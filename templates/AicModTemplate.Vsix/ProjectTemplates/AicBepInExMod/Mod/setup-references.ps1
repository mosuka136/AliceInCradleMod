# 从本地游戏安装目录填充解决方案根的 ReferenceLibrary 文件夹。
# 用法：在主工程目录下运行  .\setup-references.ps1 -GameDir "D:\Games\AliceInCradle"
# 需要游戏已安装 BepInEx与 UnityModBase（UMB）前置 Mod。
param(
    [Parameter(Mandatory = $true)]
    [string]$GameDir
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $GameDir)) {
    Write-Error "游戏目录不存在：$GameDir"
}

# 模板编译所需的引用及其来源位置。
# Core   : BepInEx\core
# Managed: 游戏数据目录 <GameName>_Data\Managed
# UMB    : BepInEx\plugins 下递归查找（UnityModBase 是前置 Mod，需要先安装）
$references = @(
    @{ Name = '0Harmony';                    Source = 'Core' },
    @{ Name = 'BepInEx';                     Source = 'Core' },
    @{ Name = 'Assembly-CSharp';             Source = 'Managed' },
    @{ Name = 'UnityEngine';                 Source = 'Managed' },
    @{ Name = 'UnityEngine.CoreModule';      Source = 'Managed' },
    @{ Name = 'UnityEngine.IMGUIModule';     Source = 'Managed' },
    @{ Name = 'UnityEngine.TextRenderingModule'; Source = 'Managed' },
    @{ Name = 'unsafeAssem';                   Source = 'Managed' },
    @{ Name = 'UnityModBase';                Source = 'UMB' },
    @{ Name = 'UnityModBase.BepInExLauncher'; Source = 'UMB' }
)

$bepInExDir = Join-Path $GameDir 'BepInEx'
$coreDir = Join-Path $bepInExDir 'core'
$pluginsDir = Join-Path $bepInExDir 'plugins'

$managedDir = Get-ChildItem -LiteralPath $GameDir -Directory -Filter '*_Data' |
    ForEach-Object { Join-Path $_.FullName 'Managed' } |
    Where-Object { Test-Path -LiteralPath $_ } |
    Select-Object -First 1

if (-not (Test-Path -LiteralPath $coreDir)) {
    Write-Error "未找到 BepInEx\core：$coreDir。请确认游戏已安装 BepInEx（目录：$bepInExDir）。"
}
if (-not $managedDir) {
    Write-Error "未找到游戏脚本目录（*_Data\Managed），请确认 -GameDir 指向游戏根目录：$GameDir"
}

# 解决方案根 = 本脚本所在工程目录的上一级
$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$targetDir = Join-Path $repoRoot 'ReferenceLibrary'

Write-Host "ReferenceLibrary : $targetDir"
Write-Host "BepInEx core     : $coreDir"
Write-Host "Game Managed     : $managedDir"
Write-Host "UMB search in    : $pluginsDir"
Write-Host ''

New-Item -ItemType Directory -Force -Path $targetDir | Out-Null

$failed = @()
foreach ($ref in $references) {
    $dllName = "$($ref.Name).dll"
    $sourceFile = $null

    switch ($ref.Source) {
        'Core'    { $p = Join-Path $coreDir $dllName;     if (Test-Path -LiteralPath $p) { $sourceFile = $p } }
        'Managed' { $p = Join-Path $managedDir $dllName;   if (Test-Path -LiteralPath $p) { $sourceFile = $p } }
        'UMB'     {
            $p = Get-ChildItem -LiteralPath $pluginsDir -Recurse -Filter $dllName -ErrorAction SilentlyContinue |
                Select-Object -First 1
            if ($p) { $sourceFile = $p.FullName }
        }
    }

    if ($sourceFile) {
        Copy-Item -LiteralPath $sourceFile -Destination (Join-Path $targetDir $dllName) -Force
        Write-Host "[OK]   $($ref.Name.PadRight(34)) <- $sourceFile"
    }
    else {
        $failed += $ref.Name
        $hint = switch ($ref.Source) {
            'Core'    { "未找到，请检查 BepInEx 安装。" }
            'Managed' { "未找到，请检查游戏版本。" }
            'UMB'     { "未找到，请先安装 UnityModBase（UMB）Mod 后重试。" }
        }
        Write-Warning "[$($ref.Source)] $dllName $hint"
    }
}

Write-Host ''
if ($failed.Count -gt 0) {
    Write-Warning "缺少 $($failed.Count) 个引用：$($failed -join ', ')，工程可能无法编译。"
    exit 1
}

Write-Host "全部引用就绪。后续步骤："
Write-Host "  1. 构建主工程（Debug）。"
Write-Host "  2. 把 bin\Debug 下的 <程序集名>.dll 拷贝到 <游戏目录>\BepInEx\plugins\ 下。"
Write-Host "  3. 启动游戏验证（BepInEx 控制台或 LogOutput.log）。"
