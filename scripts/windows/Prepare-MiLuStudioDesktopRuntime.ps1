param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [string]$NpmPath = "D:\soft\program\nodejs\npm.ps1",
    [string]$PythonPath = "",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$FrameworkDependentDotnet,
    [switch]$SkipWebBuild,
    [switch]$SkipDotnetPublish,
    [switch]$SkipPythonRuntimeBundle
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$desktopRoot = Join-Path $ProjectRoot "apps\desktop"
$runtimeRoot = Join-Path $desktopRoot "runtime"
$webDist = Join-Path $ProjectRoot "apps\web\dist"
$webRuntime = Join-Path $runtimeRoot "web"
$apiRuntime = Join-Path $runtimeRoot "control-plane\api"
$workerRuntime = Join-Path $runtimeRoot "control-plane\worker"
$migrationRuntime = Join-Path $runtimeRoot "control-plane\db\migrations"
$pythonSkillsRuntime = Join-Path $runtimeRoot "python-skills"
$pythonRuntime = Join-Path $runtimeRoot "python-runtime"

if ([string]::IsNullOrWhiteSpace($PythonPath)) {
    $PythonPath = if ($env:MILUSTUDIO_PYTHON) { $env:MILUSTUDIO_PYTHON } else { "D:\soft\program\Python\Python313\python.exe" }
}

function Assert-UnderRoot {
    param([string]$PathToCheck)

    $fullPath = [System.IO.Path]::GetFullPath($PathToCheck)
    if (-not $fullPath.StartsWith($desktopRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to modify path outside apps\desktop: $fullPath"
    }
}

function Reset-Directory {
    param([string]$PathToReset)

    Assert-UnderRoot -PathToCheck $PathToReset
    if (Test-Path -LiteralPath $PathToReset) {
        Remove-Item -LiteralPath $PathToReset -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $PathToReset | Out-Null
}

function Copy-DirectoryContents {
    param(
        [string]$Source,
        [string]$Destination
    )

    if (-not (Test-Path -LiteralPath $Source)) {
        throw "Source directory not found: $Source"
    }

    Reset-Directory -PathToReset $Destination
    Copy-Item -Path (Join-Path $Source "*") -Destination $Destination -Recurse -Force -ErrorAction Stop
}

function New-DesktopIcon {
    $brandRoot = Join-Path $ProjectRoot "apps\web\public\brand"
    $source = Join-Path $brandRoot "logo.png"
    $destination = Join-Path $desktopRoot "build\icon.ico"

    if (-not (Test-Path -LiteralPath $source)) {
        throw "Brand icon source not found: $source"
    }

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($source)
    try {
        $sizes = @(16, 24, 32, 48, 64, 128, 256)
        $entries = @()

        foreach ($size in $sizes) {
            $scaled = [System.Drawing.Bitmap]::new($size, $size)
            try {
                $graphics = [System.Drawing.Graphics]::FromImage($scaled)
                try {
                    $graphics.Clear([System.Drawing.Color]::Transparent)
                    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                    $graphics.DrawImage($bitmap, 0, 0, $size, $size)
                }
                finally {
                    $graphics.Dispose()
                }

                $pngStream = [System.IO.MemoryStream]::new()
                try {
                    $scaled.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
                    $entries += [pscustomobject]@{
                        Size = $size
                        Bytes = $pngStream.ToArray()
                    }
                }
                finally {
                    $pngStream.Dispose()
                }
            }
            finally {
                $scaled.Dispose()
            }
        }

        $stream = [System.IO.File]::Create($destination)
        $writer = [System.IO.BinaryWriter]::new($stream)
        try {
            $writer.Write([UInt16]0)
            $writer.Write([UInt16]1)
            $writer.Write([UInt16]$entries.Count)

            $offset = 6 + (16 * $entries.Count)
            foreach ($entry in $entries) {
                $iconSize = if ($entry.Size -eq 256) { 0 } else { $entry.Size }
                $writer.Write([byte]$iconSize)
                $writer.Write([byte]$iconSize)
                $writer.Write([byte]0)
                $writer.Write([byte]0)
                $writer.Write([UInt16]1)
                $writer.Write([UInt16]32)
                $writer.Write([UInt32]$entry.Bytes.Length)
                $writer.Write([UInt32]$offset)
                $offset += $entry.Bytes.Length
            }

            foreach ($entry in $entries) {
                $writer.Write([byte[]]$entry.Bytes)
            }
        }
        finally {
            $writer.Dispose()
            $stream.Dispose()
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Publish-DotnetRuntime {
    param(
        [string]$ProjectPath,
        [string]$Destination,
        [string]$Label
    )

    Reset-Directory -PathToReset $Destination

    $publishArgs = @(
        "publish",
        $ProjectPath,
        "-c",
        "Release",
        "-o",
        $Destination
    )

    if ($FrameworkDependentDotnet) {
        $publishArgs += "--no-restore"
    }
    else {
        $publishArgs += @(
            "-r",
            $RuntimeIdentifier,
            "--self-contained",
            "true",
            "/p:PublishSingleFile=false",
            "/p:PublishReadyToRun=false"
        )
    }

    & $DotnetPath @publishArgs
    if ($LASTEXITCODE -ne 0) {
        throw "$Label publish failed."
    }
}

if (-not $SkipWebBuild) {
    Push-Location (Join-Path $ProjectRoot "apps\web")
    try {
        & $NpmPath run build
        if ($LASTEXITCODE -ne 0) {
            throw "Web build failed."
        }
    }
    finally {
        Pop-Location
    }
}

if (-not $SkipDotnetPublish) {
    Publish-DotnetRuntime `
        -ProjectPath (Join-Path $ProjectRoot "backend\control-plane\src\MiLuStudio.Api\MiLuStudio.Api.csproj") `
        -Destination $apiRuntime `
        -Label "Control API"

    Publish-DotnetRuntime `
        -ProjectPath (Join-Path $ProjectRoot "backend\control-plane\src\MiLuStudio.Worker\MiLuStudio.Worker.csproj") `
        -Destination $workerRuntime `
        -Label "Worker"
}

Copy-DirectoryContents -Source $webDist -Destination $webRuntime
Copy-DirectoryContents -Source (Join-Path $ProjectRoot "backend\control-plane\db\migrations") -Destination $migrationRuntime
Copy-DirectoryContents -Source (Join-Path $ProjectRoot "backend\sidecars\python-skills") -Destination $pythonSkillsRuntime

if (-not $SkipPythonRuntimeBundle) {
    if (-not (Test-Path -LiteralPath $PythonPath)) {
        throw "Python executable not found: $PythonPath"
    }

    $pythonInstallRoot = Split-Path -Parent (Resolve-Path -LiteralPath $PythonPath).Path
    Copy-DirectoryContents -Source $pythonInstallRoot -Destination $pythonRuntime
}

New-DesktopIcon

Write-Host "MiLuStudio desktop runtime prepared at $runtimeRoot"
