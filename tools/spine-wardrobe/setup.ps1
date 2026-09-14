param([string]$Python = 'python')
$ErrorActionPreference = 'Stop'
$toolDirectory = $PSScriptRoot
$env:DOTNET_CLI_HOME = Join-Path $toolDirectory 'work\dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$virtualPython = Join-Path $toolDirectory '.venv\Scripts\python.exe'
if (-not (Test-Path -LiteralPath $virtualPython)) {
    & $Python -m venv --without-pip (Join-Path $toolDirectory '.venv')
    if ($LASTEXITCODE -ne 0) { throw 'Virtual environment creation failed.' }
}
# Bootstrap through the host pip; ensurepip can be restricted by Windows sandbox ACLs.
& $Python -m pip --python $virtualPython install --index-url https://pypi.org/simple -r (Join-Path $toolDirectory 'requirements.lock.txt')
if ($LASTEXITCODE -ne 0) { throw 'Dependency installation failed.' }
& dotnet build (Join-Path $toolDirectory 'Runtime\Runtime.csproj') --nologo
if ($LASTEXITCODE -ne 0) { throw 'Runtime build failed.' }
& dotnet msbuild (Join-Path $toolDirectory 'GameQa\GameQa.csproj') /t:Build /nologo
if ($LASTEXITCODE -ne 0) { throw 'Game QA build failed.' }
