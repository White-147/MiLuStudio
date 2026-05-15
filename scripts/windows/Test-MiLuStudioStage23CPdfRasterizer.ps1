param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$PackagePath = "",
    [string]$DownloadUrl = "",
    [string]$ExpectedSha256 = "",
    [switch]$Force,
    [switch]$RequireRuntime
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$installer = Join-Path $ProjectRoot "scripts\windows\Install-MiLuStudioPdfRasterizer.ps1"
$popplerRoot = Join-Path $ProjectRoot "runtime\poppler"
$tesseractExe = Join-Path $ProjectRoot "runtime\tesseract\tesseract.exe"
$tessdataRoot = Join-Path $ProjectRoot "runtime\tesseract\tessdata"

if (-not [string]::IsNullOrWhiteSpace($PackagePath) -or -not [string]::IsNullOrWhiteSpace($DownloadUrl) -or $Force) {
    $installArgs = @{
        ProjectRoot = $ProjectRoot
        InstallRoot = $popplerRoot
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

    & $installer @installArgs
}

$rasterizerReady = $false
try {
    & $installer -ProjectRoot $ProjectRoot -InstallRoot $popplerRoot -VerifyOnly
    $rasterizerReady = $true
}
catch {
    Write-Host $_
}

if (-not $rasterizerReady) {
    $message = "Stage 23C PDF rasterizer is not ready under $popplerRoot. Import a portable Poppler package with $installer -PackagePath <zip-or-directory>."
    if ($RequireRuntime) {
        throw $message
    }

    Write-Host $message
    Write-Host "Positive scanned-PDF rasterization was skipped because -RequireRuntime was not set."
    return
}

$ocrReady = (Test-Path -LiteralPath $tesseractExe) -and (Test-Path -LiteralPath $tessdataRoot)
if (-not $ocrReady) {
    $message = "PDF rasterizer is ready, but scanned-PDF OCR also needs Tesseract at $tesseractExe with tessdata at $tessdataRoot."
    if ($RequireRuntime) {
        throw $message
    }

    Write-Host $message
    Write-Host "Rasterizer verification passed; OCR-positive scanned PDF validation remains pending."
    return
}

Write-Host "Stage 23C PDF rasterizer runtime verification passed."
