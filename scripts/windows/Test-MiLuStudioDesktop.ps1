param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$NpmPath = "D:\soft\program\nodejs\npm.ps1",
    [switch]$SkipInstall,
    [switch]$SkipSmoke,
    [switch]$SkipApiSecurity
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$desktopRoot = Join-Path $ProjectRoot "apps\desktop"

Push-Location $desktopRoot
try {
    if (-not $SkipInstall -and -not (Test-Path -LiteralPath (Join-Path $desktopRoot "node_modules\electron"))) {
        & $NpmPath install
        if ($LASTEXITCODE -ne 0) {
            throw "Desktop npm install failed."
        }
    }

    & $NpmPath run prepare:runtime
    if ($LASTEXITCODE -ne 0) {
        throw "Desktop runtime preparation failed."
    }

    & $NpmPath run build
    if ($LASTEXITCODE -ne 0) {
        throw "Desktop TypeScript build failed."
    }

    if (-not $SkipSmoke) {
        & $NpmPath run smoke
        if ($LASTEXITCODE -ne 0) {
            throw "Desktop smoke test failed."
        }
    }

    if (-not $SkipApiSecurity) {
        powershell -ExecutionPolicy Bypass -File (Join-Path $ProjectRoot "scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1") `
            -ProjectRoot $ProjectRoot `
            -NpmPath $NpmPath `
            -SkipPrepareRuntime
        if ($LASTEXITCODE -ne 0) {
            throw "Desktop API security test failed."
        }
    }

    Write-Host "MiLuStudio desktop verification passed."
}
finally {
    Pop-Location
}
