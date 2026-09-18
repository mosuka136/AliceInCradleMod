# Package the multi-project template directory into ProjectTemplates\AicBepInExMod.zip.
# The root .vstemplate must sit at the ZIP root so Visual Studio can discover and instantiate it.
# Invoked automatically before CreateVsixContainer by the VSIX project; safe to re-run.
$ErrorActionPreference = 'Stop'

$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateDir = Join-Path $projectDir 'ProjectTemplates\AicBepInExMod'
$zipPath = Join-Path $projectDir 'ProjectTemplates\AicBepInExMod.zip'

if (-not (Test-Path -LiteralPath (Join-Path $templateDir 'AicBepInExMod.vstemplate'))) {
    Write-Error "Root vstemplate not found under $templateDir"
}

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $templateDir '*') -DestinationPath $zipPath -CompressionLevel Optimal
Write-Host "Template packaged -> $zipPath"
