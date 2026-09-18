# 真机验证：用 DTE 自动化让 Visual Studio 从模板实例化一个解决方案，
# 检查生成的文件、ProjectReference 与编译/测试是否正确。
# -TemplatePath 可验证本地模板；-GeneratedRoot 可验证通过 VS 界面创建的现有目录，跳过 DTE。
# 用法：templates\tools\test-template-vs.ps1 [-Name SampleVsMod] [-DteProgId VisualStudio.DTE.17.0]
param(
    [string]$Name = 'SampleVsMod',
    [string]$DteProgId = 'VisualStudio.DTE.17.0',
    [string]$InstanceRoot = "$env:LOCALAPPDATA\Microsoft\VisualStudio\17.0_6dc3ebf1",
    [string]$TemplatePath,
    [string]$GeneratedRoot,
    [switch]$ValidateOnly
)

$ErrorActionPreference = 'Stop'

$toolsDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$templatesRoot = Split-Path -Parent $toolsDir
$repoRoot = Split-Path -Parent $templatesRoot

# 定位已安装模板的根 vstemplate
if ($TemplatePath) {
    $vstemplate = Get-Item -LiteralPath $TemplatePath -ErrorAction Stop
}
else {
    $vstemplate = Get-ChildItem -LiteralPath (Join-Path $InstanceRoot 'Extensions') -Recurse -Filter 'AicBepInExMod.vstemplate' -ErrorAction SilentlyContinue |
        Where-Object { $_.DirectoryName -like '*ProjectTemplates*' } |
        Select-Object -First 1
}
if (-not $vstemplate) {
    Write-Error "未找到已安装的模板（Extension 目录：$InstanceRoot\Extensions）。请先安装 VSIX。"
}
Write-Host "[模板] $($vstemplate.FullName)"

$workDir = Join-Path ([System.IO.Path]::GetTempPath()) "aic-vs-instantiate-$([guid]::NewGuid().ToString('N').Substring(0,8))"
if ($GeneratedRoot) { $workDir = (Resolve-Path -LiteralPath $GeneratedRoot).Path }
else { New-Item -ItemType Directory -Force -Path $workDir | Out-Null }
Write-Host "[工作目录] $workDir"

# VS 启动期间 DTE 会拒绝调用（RPC_E_CALL_REJECTED），统一用重试封装
function Invoke-DteRetry([scriptblock]$action) {
    for ($i = 0; $i -lt 60; $i++) {
        try {
            return & $action
        }
        catch [System.Runtime.InteropServices.COMException] {
            if ($_.Exception.HResult -eq -2147418111) {
                Start-Sleep -Milliseconds 2000
                continue
            }
            throw
        }
    }
    throw 'DTE 调用重试超时（VS 未在预期时间内就绪）'
}

if (-not $GeneratedRoot) {
$dte = New-Object -ComObject $DteProgId
try {
    Write-Host '[DTE] 启动 Visual Studio，请稍候（首次扫描模板可能较慢）…'

    # 轮询等待 Solution 对象就绪（启动初期可能为 null 或拒绝调用）
    $deadline = (Get-Date).AddSeconds(180)
    $solution = $null
    while ((Get-Date) -lt $deadline) {
        try {
            $solution = $dte.Solution
            if ($null -ne $solution) { break }
        }
        catch [System.Runtime.InteropServices.COMException] {
        }
        Start-Sleep -Milliseconds 2000
    }
    if ($null -eq $solution) { throw 'DTE.Solution 在超时内未就绪' }
    Invoke-DteRetry { $dte.UserControl = $false; $dte.MainWindow.Visible = $false } | Out-Null
    Write-Host '[DTE] Solution 已就绪。'

    $solutionDir = $workDir
    $solutionFile = Join-Path $solutionDir "$Name.sln"
    Invoke-DteRetry { $dte.Solution.Create($solutionDir, "$Name.sln") } | Out-Null

    $destDir = Join-Path $solutionDir $Name
    $project = Invoke-DteRetry { $dte.Solution.AddFromTemplate($vstemplate.FullName, $destDir, $Name, $false) }

    if (-not $project) {
        Write-Host '[DTE] AddFromTemplate 未返回项目句柄（多工程模板常见），检查磁盘产物…' -ForegroundColor Yellow
    }
    else {
        Write-Host "[DTE] 已创建项目：$($project.Name)"
    }

    Invoke-DteRetry { $dte.Solution.SaveAs($solutionFile) } | Out-Null
    Invoke-DteRetry { $dte.Solution.Close($false) } | Out-Null
}
finally {
    Invoke-DteRetry { $dte.Quit() } | Out-Null
}

}
else {
    $solutions = @(Get-ChildItem -LiteralPath $workDir -Recurse -File | Where-Object { $_.Extension -in '.sln', '.slnx' })
    if ($solutions.Count -ne 1) { throw '生成目录应包含唯一的解决方案文件。' }
    $solutionFile = $solutions[0].FullName
}

# ---- 检查磁盘产物 ----
$mainDir = Join-Path $workDir $Name
$testDir = Join-Path $workDir "$Name.Test"

foreach ($p in @(
    @{ Path = Join-Path $mainDir "$Name.csproj";  Desc = '主工程 csproj' },
    @{ Path = Join-Path $mainDir "$Name.cs";      Desc = '入口类文件' },
    @{ Path = Join-Path $testDir "$Name.Test.csproj"; Desc = '测试工程 csproj' },
    @{ Path = $solutionFile;                      Desc = '解决方案文件' }
)) {
    if (-not (Test-Path -LiteralPath $p.Path)) { Write-Error "缺少 $($p.Desc)：$($p.Path)" }
    Write-Host "[OK] $($p.Desc)：$($p.Path)"
}

# 确认所有抽象出的通用源码均由 VS 实际生成，而不是仅编译空工程。
$templateRoot = $vstemplate.DirectoryName
foreach ($projectPair in @(
    @{ Source = 'Mod'; Target = $mainDir },
    @{ Source = 'Mod.Test'; Target = $testDir }
)) {
    $sourceDir = Join-Path $templateRoot $projectPair.Source
    $sourceFiles = @(Get-ChildItem -LiteralPath $sourceDir -Recurse -Filter *.cs)
    if ($sourceFiles.Count -eq 0) { throw "模板不含源码：$sourceDir" }
    foreach ($sourceFile in $sourceFiles) {
        $relativePath = $sourceFile.FullName.Substring($sourceDir.Length + 1)
        if ($relativePath -eq 'TemplatePlugin.cs') { $relativePath = "$Name.cs" }
        $targetFile = Join-Path $projectPair.Target $relativePath
        if (-not (Test-Path -LiteralPath $targetFile -PathType Leaf)) { throw "VS 未生成模板源码：$relativePath" }
    }
    Write-Host "[OK] $($projectPair.Source) 已生成 $($sourceFiles.Count) 个 C# 源码文件。"
}

# 残留占位符检查
$leftovers = Get-ChildItem -LiteralPath $workDir -Recurse -Include *.cs, *.csproj, *.sln |
    Select-String -Pattern '\$[a-z][a-z0-9_]*\$|BetterExperience' -AllMatches
if ($leftovers) {
    $leftovers | ForEach-Object { Write-Host "$($_.Path):$($_.LineNumber): $($_.Line.Trim())" }
    Write-Error '实例化产物中存在未替换的占位符或旧项目名。'
}
Write-Host '[OK] 无占位符残留。'

# 关键内容抽查：ProjectReference、命名空间、程序集名、InternalsVisibleTo、GUID
$testCsproj = Get-Content -LiteralPath (Join-Path $testDir "$Name.Test.csproj") -Raw
if ($testCsproj -notmatch [regex]::Escape("..\$Name\$Name.csproj")) {
    Write-Host '--- 测试工程 csproj 引用 ---'
    Write-Host ($testCsproj -split "`n" | Select-String 'ProjectReference')
    Write-Error "测试工程 ProjectReference 未按预期指向 ..\$Name\$Name.csproj。"
}
Write-Host '[OK] ProjectReference 正确。'

$mainCsproj = Get-Content -LiteralPath (Join-Path $mainDir "$Name.csproj") -Raw
foreach ($expect in @("<AssemblyName>$Name</AssemblyName>", "<RootNamespace>$Name</RootNamespace>")) {
    if ($mainCsproj -notcontains $expect -and $mainCsproj -notmatch [regex]::Escape($expect)) {
        Write-Error "主工程 csproj 缺少 $expect"
    }
}
Write-Host '[OK] 主工程 AssemblyName/RootNamespace 正确。'

$asmInfo = Get-Content -LiteralPath (Join-Path $mainDir 'Properties\AssemblyInfo.cs') -Raw
if ($asmInfo -notmatch [regex]::Escape("[assembly: InternalsVisibleTo(`"$Name.Test`")]")) {
    Write-Error "InternalsVisibleTo 未按预期替换为 $Name.Test：$asmInfo"
}
Write-Host '[OK] InternalsVisibleTo 正确。'

$plugin = Get-Content -LiteralPath (Join-Path $mainDir "$Name.cs") -Raw
if ($plugin -notmatch "class $Name : BaseUnityPlugin") {
    Write-Error "入口类名未按预期替换为 $Name。"
}
$patchInfo = Get-Content -LiteralPath (Join-Path $mainDir 'PatchInfo.cs') -Raw
if ($patchInfo -notmatch 'BepInPluginId = "com\.example\.' + [regex]::Escape($Name) + '"') {
    Write-Error "插件 GUID 未按预期替换：$patchInfo"
}
Write-Host '[OK] 插件标识正确。'

if ($ValidateOnly) { Write-Host 'VS 实际生成源码验证通过。'; exit 0 }

# ReferenceLibrary
Copy-Item -LiteralPath (Join-Path $repoRoot 'ReferenceLibrary') -Destination (Join-Path (Split-Path -Parent $mainDir) 'ReferenceLibrary') -Recurse
Write-Host '[环境] ReferenceLibrary 已复制。'

# 编译主工程
$msbuild = Get-ChildItem -Path 'D:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' -ErrorAction SilentlyContinue |
    Select-Object -First 1
if (-not $msbuild) {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
}
& $msbuild (Join-Path $mainDir "$Name.csproj") -restore -p:Configuration=Debug -p:UseSharedCompilation=false -v:m -nologo -nr:false | Write-Host
if ($LASTEXITCODE -ne 0) { Write-Error '实例化后的主工程编译失败。' }
Write-Host '[OK] 实例化后的主工程编译通过。'

Push-Location $testDir
try {
    dotnet test "$Name.Test.csproj" -c Debug --nologo -v q -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { Write-Error '实例化后的测试工程 dotnet test 失败。' }
}
finally { Pop-Location }
Write-Host '[OK] 实例化后的测试通过。'

Write-Host ''
Write-Host "========== 真机实例化验证全部通过（$workDir）==========" -ForegroundColor Green
exit 0
