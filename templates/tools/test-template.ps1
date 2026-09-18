# 按 vstemplate 清单模拟 Visual Studio 实例化，检查引用、编译和测试。
# 用法：.\templates\tools\test-template.ps1 [-Name SampleMod] [-Keep] [-ValidateOnly] [-MsBuild <路径>]
param(
    [ValidatePattern('^[A-Za-z_][A-Za-z0-9_]*$')]
    [string]$Name = 'SampleMod',
    [switch]$Keep,
    [switch]$ValidateOnly,
    [string]$MsBuild
)

$ErrorActionPreference = 'Stop'
$templatesRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = Split-Path -Parent $templatesRoot
$templateDir = Join-Path $templatesRoot 'AicModTemplate.Vsix\ProjectTemplates\AicBepInExMod'
$workDir = Join-Path ([System.IO.Path]::GetTempPath()) "aic-template-test-$([guid]::NewGuid().ToString('N'))"
$utf8 = New-Object System.Text.UTF8Encoding($true)

function Find-MsBuild {
    if ($MsBuild) {
        if (-not (Test-Path -LiteralPath $MsBuild -PathType Leaf)) { throw "MSBuild 不存在：$MsBuild" }
        return $MsBuild
    }
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
        $path = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe |
            Select-Object -First 1
        if ($path) { return $path }
    }
    throw '未找到 MSBuild.exe，请安装 Visual Studio 或用 -MsBuild 指定路径。'
}

function Expand-Parameters([string]$text, [hashtable]$map) {
    foreach ($key in $map.Keys) { $text = $text.Replace($key, $map[$key]) }
    return $text
}

function Copy-TemplateFile([string]$source, [string]$destination, [bool]$replace, [hashtable]$map) {
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "模板缺少文件：$source" }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    if ($replace) {
        $text = Expand-Parameters ([System.IO.File]::ReadAllText($source)) $map
        [System.IO.File]::WriteAllText($destination, $text, $utf8)
    }
    else { Copy-Item -LiteralPath $source -Destination $destination }
}

function Copy-ProjectItems([System.Xml.XmlElement]$node, [string]$sourceDir, [string]$targetDir, [hashtable]$map) {
    foreach ($item in $node.ChildNodes) {
        if ($item.LocalName -eq 'Folder') {
            $folderName = $item.GetAttribute('TargetFolderName')
            if (-not $folderName) { $folderName = $item.GetAttribute('Name') }
            Copy-ProjectItems $item (Join-Path $sourceDir $item.GetAttribute('Name')) `
                (Join-Path $targetDir (Expand-Parameters $folderName $map)) $map
        }
        elseif ($item.LocalName -eq 'ProjectItem') {
            $fileName = $item.InnerText.Trim()
            $targetName = $item.GetAttribute('TargetFileName')
            if (-not $targetName) { $targetName = $fileName }
            Copy-TemplateFile (Join-Path $sourceDir $fileName) `
                (Join-Path $targetDir (Expand-Parameters $targetName $map)) `
                ($item.GetAttribute('ReplaceParameters') -eq 'true') $map
        }
    }
}

try {
    Write-Host "工作目录：$workDir"
    [xml]$root = Get-Content -LiteralPath (Join-Path $templateDir 'AicBepInExMod.vstemplate') -Raw -Encoding UTF8
    $parentMap = @{
        '$safeprojectname$' = $Name
        '$projectname$' = $Name
        '$year$' = (Get-Date).Year.ToString()
    }
    $links = @($root.VSTemplate.TemplateContent.ProjectCollection.ProjectTemplateLink)
    if ($links.Count -ne 2) { throw '根模板应包含主工程与测试工程两个子模板。' }
    foreach ($link in $links) {
        if ($link.GetAttribute('CopyParameters') -ne 'true') { throw '子模板必须启用 CopyParameters。' }
        $projectName = Expand-Parameters $link.GetAttribute('ProjectName') $parentMap
        $childPath = Join-Path $templateDir $link.InnerText.Trim()
        [xml]$child = Get-Content -LiteralPath $childPath -Raw -Encoding UTF8
        $map = @{
            '$safeprojectname$' = $projectName
            '$projectname$' = $projectName
            '$guid1$' = [guid]::NewGuid().ToString()
            '$year$' = (Get-Date).Year.ToString()
        }
        foreach ($key in $parentMap.Keys) { $map['$ext_' + $key.Substring(1)] = $parentMap[$key] }
        foreach ($parameter in $child.VSTemplate.TemplateContent.CustomParameters.CustomParameter) {
            $map[$parameter.Name] = $parameter.Value
        }
        $project = $child.VSTemplate.TemplateContent.Project
        if ($child.VSTemplate.TemplateData.CreateInPlace -ne 'true') {
            throw 'SDK 子模板必须显式启用 CreateInPlace，避免 VS 复制时遗漏隐式源码项。'
        }
        $sourceDir = Split-Path -Parent $childPath
        $targetDir = Join-Path $workDir $projectName
        Copy-TemplateFile (Join-Path $sourceDir $project.GetAttribute('File')) `
            (Join-Path $targetDir (Expand-Parameters $project.GetAttribute('TargetFileName') $map)) `
            ($project.GetAttribute('ReplaceParameters') -eq 'true') $map
        Copy-ProjectItems $project $sourceDir $targetDir $map
    }

    $mainDir = Join-Path $workDir $Name
    $testDir = Join-Path $workDir "$Name.Test"
    $mainCsproj = Join-Path $mainDir "$Name.csproj"
    $testCsproj = Join-Path $testDir "$Name.Test.csproj"
    $leftovers = Get-ChildItem -LiteralPath $workDir -Recurse -File |
        Where-Object { $_.Extension -in '.cs', '.csproj', '.md' } |
        Select-String -Pattern '\$[a-z][a-z0-9_]*\$|BetterExperience'
    if ($leftovers) { throw "存在未替换的模板参数或旧项目名：$($leftovers -join '; ')" }
    [xml]$testProject = Get-Content -LiteralPath $testCsproj -Raw -Encoding UTF8
    $reference = @($testProject.Project.ItemGroup.ProjectReference | Where-Object { $_ })
    if ($reference.Count -ne 1 -or $reference[0].Include -ne "..\$Name\$Name.csproj") {
        throw '测试工程引用未直接生成正确的主工程路径。'
    }
    if (-not (Test-Path -LiteralPath (Join-Path $testDir $reference[0].Include))) { throw '主工程引用不存在。' }
    if ($testProject.Project.PropertyGroup.AssemblyName -ne "$Name.Test" -or
        $testProject.Project.PropertyGroup.RootNamespace -ne "$Name.Test") { throw '测试工程名称或命名空间错误。' }
    $tests = [System.IO.File]::ReadAllText((Join-Path $testDir 'BPatchGUI\NoticeStateTests.cs'))
    if (-not $tests.Contains("using $Name.BPatchGUI;") -or
        -not $tests.Contains("namespace $Name.Test.BPatchGUI")) { throw '测试源码的命名空间错误。' }
    $mainSources = @(Get-ChildItem -LiteralPath $mainDir -Recurse -Filter *.cs)
    $testSources = @(Get-ChildItem -LiteralPath $testDir -Recurse -Filter *.cs)
    if ($mainSources.Count -ne 14 -or $testSources.Count -ne 2) { throw '生成的通用源码或测试源码不完整。' }
    Write-Host "[检查] 主工程 $($mainSources.Count) 个源码、测试工程 $($testSources.Count) 个源码，参数、文件名与工程引用正确。"

    if (-not $ValidateOnly) {
        $repoRefLib = Join-Path $repoRoot 'ReferenceLibrary'
        if (-not (Test-Path -LiteralPath $repoRefLib -PathType Container)) {
            throw '仓库缺少 ReferenceLibrary；请先填充引用，或用 -ValidateOnly 仅验证实例化。'
        }
        Copy-Item -LiteralPath $repoRefLib -Destination (Join-Path $workDir 'ReferenceLibrary') -Recurse
        $msbuildPath = Find-MsBuild
        & $msbuildPath $mainCsproj -restore -p:Configuration=Debug -p:UseSharedCompilation=false -v:m -nologo -nr:false
        if ($LASTEXITCODE -ne 0) { throw '主工程编译失败。' }
        if (-not (Test-Path -LiteralPath (Join-Path $mainDir "bin\Debug\$Name.dll"))) { throw '主工程未生成 DLL。' }
        dotnet test $testCsproj -c Debug --nologo -v q -p:UseSharedCompilation=false
        if ($LASTEXITCODE -ne 0) { throw '测试工程执行失败。' }
    }

    if ($Keep) { Write-Host "临时目录保留：$workDir" }
    else {
        $resolvedWorkDir = [System.IO.Path]::GetFullPath($workDir)
        $tempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd('\') + '\'
        if (-not $resolvedWorkDir.StartsWith($tempRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
            [System.IO.Path]::GetFileName($resolvedWorkDir) -notmatch '^aic-template-test-[0-9a-f]{32}$') {
            throw "临时目录超出预期范围：$resolvedWorkDir"
        }
        Remove-Item -LiteralPath $resolvedWorkDir -Recurse -Force
    }
    Write-Host '模板离线自检通过。' -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "自检失败：$($_.Exception.Message)" -ForegroundColor Red
    Write-Host "临时目录保留：$workDir"
    exit 1
}
