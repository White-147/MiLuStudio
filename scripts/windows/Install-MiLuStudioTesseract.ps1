param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$InstallRoot = "",
    [string]$PackagePath = "",
    [string]$DownloadUrl = "",
    [string]$ExpectedSha256 = "",
    [switch]$Force,
    [switch]$VerifyOnly,
    [switch]$AllowInstaller
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

function Get-TesseractRuntimeRoot([string]$ExecutablePath) {
    $current = Split-Path -Parent $ExecutablePath
    for ($i = 0; $i -lt 4; $i++) {
        if ([string]::IsNullOrWhiteSpace($current)) {
            break
        }

        if (Test-Path -LiteralPath (Join-Path $current "tessdata")) {
            return $current
        }

        $parent = Split-Path -Parent $current
        if ($parent -eq $current) {
            break
        }

        $current = $parent
    }

    Split-Path -Parent $ExecutablePath
}

function Find-TesseractExecutable([string]$SearchRoot) {
    if (-not (Test-Path -LiteralPath $SearchRoot)) {
        return $null
    }

    $directPath = Join-Path $SearchRoot "tesseract.exe"
    if (Test-Path -LiteralPath $directPath) {
        return (Get-Item -LiteralPath $directPath).FullName
    }

    $match = Get-ChildItem -LiteralPath $SearchRoot -Recurse -Filter "tesseract.exe" -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($null -eq $match) {
        return $null
    }

    $match.FullName
}

function Get-TesseractRuntimeInfo([string]$RootPath) {
    $executablePath = Find-TesseractExecutable -SearchRoot $RootPath
    $tessdataPath = $null
    if (-not [string]::IsNullOrWhiteSpace($executablePath)) {
        $runtimeRoot = Get-TesseractRuntimeRoot -ExecutablePath $executablePath
        $candidateTessdata = Join-Path $runtimeRoot "tessdata"
        if (Test-Path -LiteralPath $candidateTessdata) {
            $tessdataPath = (Get-Item -LiteralPath $candidateTessdata).FullName
        }
    }
    else {
        $candidateTessdata = Join-Path $RootPath "tessdata"
        if (Test-Path -LiteralPath $candidateTessdata) {
            $tessdataPath = (Get-Item -LiteralPath $candidateTessdata).FullName
        }
    }

    $languages = @()
    if (-not [string]::IsNullOrWhiteSpace($tessdataPath)) {
        $languages = @(Get-ChildItem -LiteralPath $tessdataPath -Filter "*.traineddata" -ErrorAction SilentlyContinue |
            ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_.Name) } |
            Sort-Object)
    }

    [pscustomobject]@{
        ExecutablePath = $executablePath
        TessdataPath = $tessdataPath
        Languages = $languages
        HasEnglish = $languages -contains "eng"
        HasSimplifiedChinese = $languages -contains "chi_sim"
    }
}

function Test-TesseractRuntime([string]$RootPath) {
    $info = Get-TesseractRuntimeInfo -RootPath $RootPath
    if ([string]::IsNullOrWhiteSpace($info.ExecutablePath) -or -not (Test-Path -LiteralPath $info.ExecutablePath)) {
        Write-Host "Tesseract executable was not found under $RootPath"
        return $false
    }

    $versionOutput = & $info.ExecutablePath --version 2>&1
    $versionLine = @($versionOutput | Select-Object -First 1)[0]
    Write-Host "Tesseract executable: $($info.ExecutablePath)"
    Write-Host "Tesseract version: $versionLine"

    if ([string]::IsNullOrWhiteSpace($info.TessdataPath) -or -not (Test-Path -LiteralPath $info.TessdataPath)) {
        Write-Host "Tessdata directory was not found next to the runtime."
        return $false
    }

    Write-Host "Tessdata directory: $($info.TessdataPath)"
    Write-Host "Languages: $([string]::Join(', ', $info.Languages))"
    if (-not $info.HasEnglish) {
        Write-Host "eng.traineddata is missing; OCR fallback will be unreliable for validation fixtures."
        return $false
    }

    return $true
}

function Write-InstallManifest(
    [string]$RootPath,
    [string]$SourceType,
    [string]$Source,
    [string]$SourceSha256
) {
    $info = Get-TesseractRuntimeInfo -RootPath $RootPath
    $manifest = [ordered]@{
        installedAt = (Get-Date).ToString("o")
        sourceType = $SourceType
        source = $Source
        installRoot = $RootPath
        tesseractPath = $info.ExecutablePath
        tessdataPath = $info.TessdataPath
        languages = @($info.Languages)
        hasEnglish = $info.HasEnglish
        hasSimplifiedChinese = $info.HasSimplifiedChinese
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
    $executablePath = Find-TesseractExecutable -SearchRoot $SourceRoot
    if ([string]::IsNullOrWhiteSpace($executablePath)) {
        throw "Source does not contain tesseract.exe: $SourceRoot"
    }

    $runtimeRoot = Get-TesseractRuntimeRoot -ExecutablePath $executablePath
    if (Test-Path -LiteralPath $TargetRoot) {
        Remove-Item -LiteralPath $TargetRoot -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $TargetRoot | Out-Null
    Get-ChildItem -LiteralPath $runtimeRoot -Force | Copy-Item -Destination $TargetRoot -Recurse -Force
    Write-InstallManifest -RootPath $TargetRoot -SourceType $SourceType -Source $Source -SourceSha256 $SourceSha256
}

function Install-FromInstaller(
    [string]$InstallerPath,
    [string]$TargetRoot,
    [string]$Source,
    [string]$SourceSha256
) {
    if (-not $AllowInstaller) {
        throw "Installer packages require -AllowInstaller. Prefer a portable ZIP or unpacked directory when possible."
    }

    if (Test-Path -LiteralPath $TargetRoot) {
        Remove-Item -LiteralPath $TargetRoot -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $TargetRoot | Out-Null
    $arguments = @(
        "/VERYSILENT",
        "/SUPPRESSMSGBOXES",
        "/NORESTART",
        "/DIR=$TargetRoot"
    )
    $process = Start-Process -FilePath $InstallerPath -ArgumentList $arguments -Wait -WindowStyle Hidden -PassThru
    if ($process.ExitCode -ne 0) {
        throw "Tesseract installer exited with code $($process.ExitCode)."
    }

    $installedExecutable = Find-TesseractExecutable -SearchRoot $TargetRoot
    if ([string]::IsNullOrWhiteSpace($installedExecutable)) {
        throw "Installer completed but tesseract.exe was not found under $TargetRoot."
    }

    Write-InstallManifest -RootPath $TargetRoot -SourceType "installer" -Source $Source -SourceSha256 $SourceSha256
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
    $InstallRoot = Join-Path $resolvedProjectRoot "runtime\tesseract"
}

$resolvedInstallRoot = Assert-WithinRoot $InstallRoot $resolvedProjectRoot
$downloadRoot = Assert-WithinRoot (Join-Path $resolvedProjectRoot ".tmp\tesseract-download") $resolvedProjectRoot
$extractRoot = Join-Path $downloadRoot "extract"

if ($VerifyOnly) {
    if (Test-TesseractRuntime -RootPath $resolvedInstallRoot) {
        return
    }

    throw "Tesseract runtime verification failed at $resolvedInstallRoot"
}

if ((Test-Path -LiteralPath $resolvedInstallRoot) -and -not $Force) {
    if (Test-TesseractRuntime -RootPath $resolvedInstallRoot) {
        Write-Host "Tesseract already installed at $resolvedInstallRoot. Use -Force to reinstall."
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
        $fileName = "tesseract-package.bin"
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
    elseif ($extension -ieq ".exe") {
        Install-FromInstaller -InstallerPath $sourceItem.FullName -TargetRoot $resolvedInstallRoot -Source $sourceLabel -SourceSha256 $sourceSha256
    }
    else {
        throw "Unsupported Tesseract package extension '$extension'. Use an unpacked directory, ZIP, or installer with -AllowInstaller."
    }
}

if (-not (Test-TesseractRuntime -RootPath $resolvedInstallRoot)) {
    throw "Tesseract was installed but verification did not pass."
}

Write-Host "Tesseract runtime installed at $resolvedInstallRoot"
