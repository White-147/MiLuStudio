param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$InstallRoot = "",
    [string]$PackagePath = "",
    [string]$DownloadUrl = "",
    [string]$ExpectedSha256 = "",
    [switch]$Force,
    [switch]$VerifyOnly
)

$ErrorActionPreference = "Stop"

function Resolve-StrictPath([string]$PathValue) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PathValue)
}

function Assert-WithinRoot([string]$PathValue, [string]$RootValue) {
    $resolvedPath = Resolve-StrictPath $PathValue
    $resolvedRoot = Resolve-StrictPath $RootValue
    if (-not $resolvedPath.StartsWith($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to write outside project root. Path: $resolvedPath Root: $resolvedRoot"
    }

    $resolvedPath
}

function Find-PdfRasterizerExecutable([string]$SearchRoot) {
    if (-not (Test-Path -LiteralPath $SearchRoot)) {
        return $null
    }

    $libraryPath = Join-Path $SearchRoot "Library\bin\pdftoppm.exe"
    if (Test-Path -LiteralPath $libraryPath) {
        return (Get-Item -LiteralPath $libraryPath).FullName
    }

    $binPath = Join-Path $SearchRoot "bin\pdftoppm.exe"
    if (Test-Path -LiteralPath $binPath) {
        return (Get-Item -LiteralPath $binPath).FullName
    }

    $directPath = Join-Path $SearchRoot "pdftoppm.exe"
    if (Test-Path -LiteralPath $directPath) {
        return (Get-Item -LiteralPath $directPath).FullName
    }

    $match = Get-ChildItem -LiteralPath $SearchRoot -Recurse -Filter "pdftoppm.exe" -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($null -eq $match) {
        return $null
    }

    $match.FullName
}

function Get-PdfRasterizerRuntimeRoot([string]$ExecutablePath) {
    $binRoot = Split-Path -Parent $ExecutablePath
    $parent = Split-Path -Parent $binRoot
    $grandParent = Split-Path -Parent $parent

    if ((Split-Path -Leaf $binRoot) -ieq "bin" -and (Split-Path -Leaf $parent) -ieq "Library") {
        return $grandParent
    }

    if ((Split-Path -Leaf $binRoot) -ieq "bin") {
        return $parent
    }

    return $binRoot
}

function Test-PdfRasterizerRuntime([string]$RootPath) {
    $executablePath = Find-PdfRasterizerExecutable -SearchRoot $RootPath
    if ([string]::IsNullOrWhiteSpace($executablePath) -or -not (Test-Path -LiteralPath $executablePath)) {
        Write-Host "pdftoppm.exe was not found under $RootPath"
        return $false
    }

    $versionOutput = & $executablePath -v 2>&1
    $versionLine = @($versionOutput | Select-Object -First 1)[0]
    Write-Host "PDF rasterizer executable: $executablePath"
    Write-Host "PDF rasterizer version: $versionLine"
    return $true
}

function Write-InstallManifest(
    [string]$RootPath,
    [string]$SourceType,
    [string]$Source,
    [string]$SourceSha256
) {
    $executablePath = Find-PdfRasterizerExecutable -SearchRoot $RootPath
    $manifest = [ordered]@{
        installedAt = (Get-Date).ToString("o")
        sourceType = $SourceType
        source = $Source
        installRoot = $RootPath
        pdftoppmPath = $executablePath
        sourceSha256 = $SourceSha256
    }

    $manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $RootPath "manifest.json") -Encoding UTF8
}

function Install-FromDirectory(
    [string]$SourceRoot,
    [string]$TargetRoot,
    [string]$SourceType,
    [string]$Source,
    [string]$SourceSha256
) {
    $executablePath = Find-PdfRasterizerExecutable -SearchRoot $SourceRoot
    if ([string]::IsNullOrWhiteSpace($executablePath)) {
        throw "Source does not contain pdftoppm.exe: $SourceRoot"
    }

    $runtimeRoot = Get-PdfRasterizerRuntimeRoot -ExecutablePath $executablePath
    if (Test-Path -LiteralPath $TargetRoot) {
        Remove-Item -LiteralPath $TargetRoot -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $TargetRoot | Out-Null
    Get-ChildItem -LiteralPath $runtimeRoot -Force | Copy-Item -Destination $TargetRoot -Recurse -Force
    Write-InstallManifest -RootPath $TargetRoot -SourceType $SourceType -Source $Source -SourceSha256 $SourceSha256
}

function Download-File([string]$Url, [string]$Destination) {
    Write-Host "Downloading $Url"
    Invoke-WebRequest -Uri $Url -OutFile $Destination -UseBasicParsing -TimeoutSec 180
}

function Assert-ExpectedSha256([string]$PathValue, [string]$ExpectedHash) {
    if ([string]::IsNullOrWhiteSpace($ExpectedHash)) {
        return
    }

    $actual = (Get-FileHash -LiteralPath $PathValue -Algorithm SHA256).Hash.ToLowerInvariant()
    $expected = $ExpectedHash.Trim().ToLowerInvariant()
    if ($actual -ne $expected) {
        throw "SHA256 mismatch. Expected $expected but got $actual"
    }

    Write-Host "SHA256 verified."
}

$resolvedProjectRoot = Resolve-StrictPath $ProjectRoot
if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $resolvedProjectRoot "runtime\poppler"
}

$resolvedInstallRoot = Assert-WithinRoot $InstallRoot $resolvedProjectRoot
$downloadRoot = Assert-WithinRoot (Join-Path $resolvedProjectRoot ".tmp\poppler-download") $resolvedProjectRoot
$extractRoot = Join-Path $downloadRoot "extract"

if ($VerifyOnly) {
    if (Test-PdfRasterizerRuntime -RootPath $resolvedInstallRoot) {
        return
    }

    throw "PDF rasterizer runtime verification failed at $resolvedInstallRoot"
}

if ((Test-Path -LiteralPath $resolvedInstallRoot) -and -not $Force) {
    if (Test-PdfRasterizerRuntime -RootPath $resolvedInstallRoot) {
        Write-Host "PDF rasterizer already installed at $resolvedInstallRoot. Use -Force to reinstall."
        return
    }

    throw "Install root exists but runtime is incomplete. Use -Force to replace $resolvedInstallRoot."
}

New-Item -ItemType Directory -Force -Path $downloadRoot | Out-Null

$sourcePath = ""
$sourceType = ""
$sourceLabel = ""
$sourceSha256 = ""
if (-not [string]::IsNullOrWhiteSpace($PackagePath)) {
    $sourcePath = (Resolve-Path -LiteralPath $PackagePath).Path
    $sourceType = if ((Get-Item -LiteralPath $sourcePath).PSIsContainer) { "directory" } else { "package" }
    $sourceLabel = $sourcePath
    if (-not (Get-Item -LiteralPath $sourcePath).PSIsContainer) {
        Assert-ExpectedSha256 -PathValue $sourcePath -ExpectedHash $ExpectedSha256
        $sourceSha256 = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}
elseif (-not [string]::IsNullOrWhiteSpace($DownloadUrl)) {
    $uri = [System.Uri]$DownloadUrl
    $fileName = [System.IO.Path]::GetFileName($uri.AbsolutePath)
    if ([string]::IsNullOrWhiteSpace($fileName)) {
        $fileName = "poppler-package.bin"
    }

    $sourcePath = Join-Path $downloadRoot $fileName
    Download-File -Url $DownloadUrl -Destination $sourcePath
    Assert-ExpectedSha256 -PathValue $sourcePath -ExpectedHash $ExpectedSha256
    $sourceSha256 = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
    $sourceType = "download"
    $sourceLabel = $DownloadUrl
}
else {
    throw "Provide -PackagePath for an offline directory/ZIP, or -DownloadUrl for an explicit auxiliary download."
}

$sourceItem = Get-Item -LiteralPath $sourcePath
if ($sourceItem.PSIsContainer) {
    Install-FromDirectory -SourceRoot $sourceItem.FullName -TargetRoot $resolvedInstallRoot -SourceType $sourceType -Source $sourceLabel -SourceSha256 $sourceSha256
}
else {
    $extension = [System.IO.Path]::GetExtension($sourceItem.FullName)
    if ($extension -ieq ".zip") {
        if (Test-Path -LiteralPath $extractRoot) {
            Remove-Item -LiteralPath $extractRoot -Recurse -Force
        }

        New-Item -ItemType Directory -Force -Path $extractRoot | Out-Null
        Expand-Archive -LiteralPath $sourceItem.FullName -DestinationPath $extractRoot -Force
        Install-FromDirectory -SourceRoot $extractRoot -TargetRoot $resolvedInstallRoot -SourceType $sourceType -Source $sourceLabel -SourceSha256 $sourceSha256
    }
    else {
        throw "Unsupported Poppler package extension '$extension'. Use an unpacked directory or ZIP."
    }
}

if (-not (Test-PdfRasterizerRuntime -RootPath $resolvedInstallRoot)) {
    throw "PDF rasterizer was installed but verification did not pass."
}

Write-Host "PDF rasterizer runtime installed at $resolvedInstallRoot"
