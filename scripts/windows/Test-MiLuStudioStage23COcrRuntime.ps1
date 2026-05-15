param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5399",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [string]$PackagePath = "",
    [string]$DownloadUrl = "",
    [string]$ExpectedSha256 = "",
    [switch]$Force,
    [switch]$AllowInstaller,
    [switch]$RequireRuntime,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$installer = Join-Path $ProjectRoot "scripts\windows\Install-MiLuStudioTesseract.ps1"
$assetParsingTest = Join-Path $ProjectRoot "scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1"
$tesseractRoot = Join-Path $ProjectRoot "runtime\tesseract"
$tesseractExe = Join-Path $tesseractRoot "tesseract.exe"
$tessdataRoot = Join-Path $tesseractRoot "tessdata"

if (-not [string]::IsNullOrWhiteSpace($PackagePath) -or -not [string]::IsNullOrWhiteSpace($DownloadUrl) -or $Force) {
    $installArgs = @{
        ProjectRoot = $ProjectRoot
        InstallRoot = $tesseractRoot
    }
    if (-not [string]::IsNullOrWhiteSpace($PackagePath)) {
        $installArgs.PackagePath = $PackagePath
    }
    if (-not [string]::IsNullOrWhiteSpace($DownloadUrl)) {
        $installArgs.DownloadUrl = $DownloadUrl
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedSha256)) {
        $installArgs.ExpectedSha256 = $ExpectedSha256
    }
    if ($Force) {
        $installArgs.Force = $true
    }
    if ($AllowInstaller) {
        $installArgs.AllowInstaller = $true
    }

    & $installer @installArgs
}

$runtimeReady = $false
try {
    & $installer -ProjectRoot $ProjectRoot -InstallRoot $tesseractRoot -VerifyOnly
    $runtimeReady = $true
}
catch {
    Write-Host $_
}

if (-not $runtimeReady) {
    $message = "Stage 23C OCR runtime is not ready at $tesseractExe with tessdata at $tessdataRoot. Import a portable Tesseract package with $installer -PackagePath <zip-or-directory>."
    if ($RequireRuntime) {
        throw $message
    }

    Write-Host $message
    Write-Host "Positive OCR integration was skipped because -RequireRuntime was not set."
    return
}

$testArgs = @{
    ProjectRoot = $ProjectRoot
    ApiBaseUrl = $ApiBaseUrl
    DotnetPath = $DotnetPath
    RequireOcrRuntime = $true
}
if ($SkipBuild) {
    $testArgs.SkipBuild = $true
}

& $assetParsingTest @testArgs
Write-Host "Stage 23C OCR runtime verification passed."
