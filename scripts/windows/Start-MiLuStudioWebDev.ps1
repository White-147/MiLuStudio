param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$NpmPath = "D:\soft\program\nodejs\npm.ps1",
    [switch]$SkipServiceBuild
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$serviceScript = Join-Path $ProjectRoot "scripts\windows\Start-MiLuStudioLocalServices.ps1"

if ($SkipServiceBuild) {
    powershell -ExecutionPolicy Bypass -File $serviceScript -ProjectRoot $ProjectRoot -SkipBuild
}
else {
    powershell -ExecutionPolicy Bypass -File $serviceScript -ProjectRoot $ProjectRoot
}

Push-Location (Join-Path $ProjectRoot "apps\web")
try {
    & $NpmPath run dev
}
finally {
    Pop-Location
}
