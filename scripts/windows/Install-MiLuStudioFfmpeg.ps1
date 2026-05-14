param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$InstallRoot = "D:\code\MiLuStudio\runtime\ffmpeg",
    [string[]]$SourceUrls = @(
        "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip",
        "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"
    ),
    [switch]$Force
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

function Download-File([string]$Url, [string]$Destination) {
    Write-Host "Downloading $Url"
    Invoke-WebRequest -Uri $Url -OutFile $Destination -UseBasicParsing -TimeoutSec 120
}

function Try-Verify-Sha256([string]$Url, [string]$ArchivePath) {
    $shaUrl = "$Url.sha256"
    $shaPath = "$ArchivePath.sha256"
    try {
        Invoke-WebRequest -Uri $shaUrl -OutFile $shaPath -UseBasicParsing -TimeoutSec 30
        $expected = (Get-Content -LiteralPath $shaPath -Raw).Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)[0].Trim().ToLowerInvariant()
        $actual = (Get-FileHash -LiteralPath $ArchivePath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($expected -and $actual -ne $expected) {
            throw "SHA256 mismatch. Expected $expected but got $actual"
        }
        if ($expected) {
            Write-Host "SHA256 verified."
        }
    } catch {
        Write-Host "SHA256 sidecar unavailable or not verifiable; continuing with HTTPS source fallback policy."
    }
}

$resolvedProjectRoot = Resolve-StrictPath $ProjectRoot
$resolvedInstallRoot = Assert-WithinRoot $InstallRoot $resolvedProjectRoot
$downloadRoot = Assert-WithinRoot (Join-Path $resolvedProjectRoot ".tmp\ffmpeg-download") $resolvedProjectRoot
$extractRoot = Join-Path $downloadRoot "extract"

if ((Test-Path -LiteralPath (Join-Path $resolvedInstallRoot "bin\ffmpeg.exe")) -and -not $Force) {
    & (Join-Path $resolvedInstallRoot "bin\ffmpeg.exe") -version
    Write-Host "FFmpeg already installed at $resolvedInstallRoot. Use -Force to reinstall."
    exit 0
}

New-Item -ItemType Directory -Force -Path $downloadRoot | Out-Null

$lastError = $null
foreach ($url in $SourceUrls) {
    $archivePath = Join-Path $downloadRoot ("ffmpeg-" + [Guid]::NewGuid().ToString("N") + ".zip")
    try {
        if (Test-Path -LiteralPath $extractRoot) {
            Remove-Item -LiteralPath $extractRoot -Recurse -Force
        }
        New-Item -ItemType Directory -Force -Path $extractRoot | Out-Null

        Download-File $url $archivePath
        Try-Verify-Sha256 $url $archivePath
        Expand-Archive -LiteralPath $archivePath -DestinationPath $extractRoot -Force

        $ffmpeg = Get-ChildItem -LiteralPath $extractRoot -Recurse -Filter "ffmpeg.exe" | Select-Object -First 1
        $ffprobe = Get-ChildItem -LiteralPath $extractRoot -Recurse -Filter "ffprobe.exe" | Select-Object -First 1
        if (-not $ffmpeg -or -not $ffprobe) {
            throw "Archive did not contain ffmpeg.exe and ffprobe.exe."
        }

        $buildRoot = Split-Path -Parent (Split-Path -Parent $ffmpeg.FullName)
        if (Test-Path -LiteralPath $resolvedInstallRoot) {
            Remove-Item -LiteralPath $resolvedInstallRoot -Recurse -Force
        }
        New-Item -ItemType Directory -Force -Path $resolvedInstallRoot | Out-Null
        Get-ChildItem -LiteralPath $buildRoot | Copy-Item -Destination $resolvedInstallRoot -Recurse -Force

        $manifest = [ordered]@{
            installedAt = (Get-Date).ToString("o")
            sourceUrl = $url
            installRoot = $resolvedInstallRoot
            ffmpegPath = Join-Path $resolvedInstallRoot "bin\ffmpeg.exe"
            ffprobePath = Join-Path $resolvedInstallRoot "bin\ffprobe.exe"
            sha256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
        }
        $manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $resolvedInstallRoot "manifest.json") -Encoding UTF8

        & (Join-Path $resolvedInstallRoot "bin\ffmpeg.exe") -version
        Write-Host "FFmpeg installed at $resolvedInstallRoot"
        exit 0
    } catch {
        $lastError = $_
        Write-Host "Source failed: $url"
        Write-Host $_
    }
}

throw "Unable to install FFmpeg from all configured sources. Last error: $lastError"
