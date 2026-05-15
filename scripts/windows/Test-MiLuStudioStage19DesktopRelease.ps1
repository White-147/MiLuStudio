param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$NpmPath = "D:\soft\program\nodejs\npm.ps1",
    [string]$InstallerPath = "",
    [string]$UnpackedRoot = "",
    [int64]$MinimumInstallerBytes = 10000000,
    [switch]$BuildPackage,
    [switch]$RequireSigned,
    [switch]$SkipDesktopBuild,
    [switch]$SkipApiSecurity
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$desktopRoot = Join-Path $ProjectRoot "apps\desktop"
$packagePath = Join-Path $desktopRoot "package.json"
$mainPath = Join-Path $desktopRoot "src\main.ts"
$pathsPath = Join-Path $desktopRoot "src\paths.ts"
$preloadPath = Join-Path $desktopRoot "src\preload.ts"
$webHostPath = Join-Path $desktopRoot "src\webHost.ts"
$installerScriptPath = Join-Path $desktopRoot "build\installer.nsh"
$reportPath = Join-Path $ProjectRoot ".tmp\stage19-desktop-release-report.json"

$warnings = [System.Collections.Generic.List[string]]::new()
$checks = [System.Collections.Generic.List[object]]::new()

function Write-Stage19Pass {
    param([string]$Label)

    $checks.Add([pscustomobject]@{
        label = $Label
        status = "passed"
    }) | Out-Null
    Write-Host "[pass] $Label"
}

function Write-Stage19Warning {
    param([string]$Message)

    $warnings.Add($Message) | Out-Null
    Write-Warning $Message
}

function Assert-Condition {
    param(
        [bool]$Condition,
        [string]$Message,
        [string]$Label = $Message
    )

    if (-not $Condition) {
        throw $Message
    }

    Write-Stage19Pass -Label $Label
}

function Assert-Equal {
    param(
        [object]$Actual,
        [object]$Expected,
        [string]$Label
    )

    if ($Actual -ne $Expected) {
        throw "$Label expected '$Expected' but found '$Actual'."
    }

    Write-Stage19Pass -Label $Label
}

function Assert-RequiredFile {
    param(
        [string]$Path,
        [string]$Label,
        [int64]$MinimumBytes = 1
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Label not found: $Path"
    }

    $item = Get-Item -LiteralPath $Path
    if ($item.Length -lt $MinimumBytes) {
        throw "$Label is too small: $Path ($($item.Length) bytes)."
    }

    Write-Stage19Pass -Label $Label
}

function Assert-RequiredDirectory {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Label not found: $Path"
    }

    Write-Stage19Pass -Label $Label
}

function Assert-AnyFile {
    param(
        [string[]]$Paths,
        [string]$Label
    )

    foreach ($path in $Paths) {
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            Write-Stage19Pass -Label $Label
            return
        }
    }

    throw "$Label not found. Checked: $($Paths -join ', ')"
}

function Assert-RequiredText {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Label
    )

    if ($Text -notmatch $Pattern) {
        throw "$Label missing pattern: $Pattern"
    }

    Write-Stage19Pass -Label $Label
}

function Assert-ForbiddenText {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Label
    )

    if ($Text -match $Pattern) {
        throw "$Label contains forbidden pattern: $Pattern"
    }

    Write-Stage19Pass -Label $Label
}

function Get-RelativeFullPath {
    param(
        [string]$BasePath,
        [string]$RelativePath
    )

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $RelativePath))
}

function Test-Signature {
    param(
        [string]$Path,
        [string]$Label
    )

    $signature = Get-AuthenticodeSignature -LiteralPath $Path
    $status = [string]$signature.Status
    $subject = if ($signature.SignerCertificate) { $signature.SignerCertificate.Subject } else { "" }

    if ($RequireSigned -and $status -ne "Valid") {
        throw "$Label must be Authenticode signed for this run, but status is '$status'."
    }

    if ($status -eq "Valid") {
        Write-Stage19Pass -Label "$Label Authenticode signature is valid"
    }
    else {
        Write-Stage19Warning "$Label Authenticode status is '$status'. This is acceptable for local Stage 19 validation, but blocks a formal public release."
    }

    return [pscustomobject]@{
        label = $Label
        path = $Path
        status = $status
        subject = $subject
    }
}

if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) {
    throw "Desktop package.json not found: $packagePath"
}

$package = Get-Content -LiteralPath $packagePath -Raw | ConvertFrom-Json

if (-not $SkipDesktopBuild) {
    Push-Location $desktopRoot
    try {
        & $NpmPath run build
        if ($LASTEXITCODE -ne 0) {
            throw "Desktop TypeScript build failed."
        }
    }
    finally {
        Pop-Location
    }
}

if ($BuildPackage) {
    Push-Location $desktopRoot
    try {
        & $NpmPath run dist:win
        if ($LASTEXITCODE -ne 0) {
            throw "Desktop Windows package build failed."
        }
    }
    finally {
        Pop-Location
    }
}

$outputRoot = Get-RelativeFullPath -BasePath $desktopRoot -RelativePath $package.build.directories.output
$expectedInstallerName = "MiLuStudio-Setup-$($package.version).exe"

if ([string]::IsNullOrWhiteSpace($InstallerPath)) {
    $InstallerPath = Join-Path $outputRoot $expectedInstallerName
}

if ([string]::IsNullOrWhiteSpace($UnpackedRoot)) {
    $UnpackedRoot = Join-Path $outputRoot "win-unpacked"
}

$InstallerPath = [System.IO.Path]::GetFullPath($InstallerPath)
$UnpackedRoot = [System.IO.Path]::GetFullPath($UnpackedRoot)
$resourcesRoot = Join-Path $UnpackedRoot "resources"

Assert-Equal -Actual $package.name -Expected "@milu-studio/desktop" -Label "desktop package name"
Assert-Equal -Actual $package.main -Expected "dist/main.js" -Label "desktop main entry"
Assert-Equal -Actual $package.build.appId -Expected "com.milustudio.desktop" -Label "electron-builder appId"
Assert-Equal -Actual $package.build.productName -Expected "MiLuStudio" -Label "electron-builder productName"
Assert-Equal -Actual $package.build.artifactName -Expected 'MiLuStudio-Setup-${version}.${ext}' -Label "installer artifact name"
Assert-Equal -Actual $package.build.asar -Expected $true -Label "asar packaging enabled"
Assert-Equal -Actual $package.build.win.requestedExecutionLevel -Expected "asInvoker" -Label "installer requestedExecutionLevel"
Assert-Condition -Condition (@($package.build.win.target) -contains "nsis") -Message "Windows target must include nsis." -Label "Windows target includes NSIS"
Assert-Equal -Actual $package.build.nsis.oneClick -Expected $false -Label "NSIS assisted installer"
Assert-Equal -Actual $package.build.nsis.allowToChangeInstallationDirectory -Expected $true -Label "NSIS custom install path"
Assert-Equal -Actual $package.build.nsis.createDesktopShortcut -Expected $true -Label "NSIS desktop shortcut"
Assert-Equal -Actual $package.build.nsis.createStartMenuShortcut -Expected $true -Label "NSIS Start Menu shortcut"
Assert-Equal -Actual $package.build.nsis.deleteAppDataOnUninstall -Expected $false -Label "NSIS keeps app data on uninstall"
Assert-Equal -Actual $package.build.nsis.shortcutName -Expected "MiLuStudio" -Label "NSIS shortcut name"

$resourceTargets = @($package.build.extraResources | ForEach-Object { $_.to })
foreach ($target in @("web", "control-plane", "python-skills", "python-runtime", "build/icon.ico")) {
    Assert-Condition -Condition ($resourceTargets -contains $target) -Message "extraResources must include $target." -Label "extraResources includes $target"
}

$mainText = Get-Content -LiteralPath $mainPath -Raw
$pathsText = Get-Content -LiteralPath $pathsPath -Raw
$preloadText = Get-Content -LiteralPath $preloadPath -Raw
$webHostText = Get-Content -LiteralPath $webHostPath -Raw
$installerScriptText = Get-Content -LiteralPath $installerScriptPath -Raw

Assert-RequiredText -Text $mainText -Pattern "const appId = 'com\.milustudio\.desktop'" -Label "main appId matches installer"
Assert-RequiredText -Text $mainText -Pattern "app\.setAppUserModelId\(appId\)" -Label "AppUserModelID is set"
Assert-RequiredText -Text $mainText -Pattern "contextIsolation:\s*true" -Label "renderer context isolation"
Assert-RequiredText -Text $mainText -Pattern "sandbox:\s*true" -Label "renderer sandbox"
Assert-RequiredText -Text $mainText -Pattern "nodeIntegration:\s*false" -Label "renderer Node integration disabled"
Assert-RequiredText -Text $mainText -Pattern "webSecurity:\s*true" -Label "renderer web security"
Assert-RequiredText -Text $mainText -Pattern "setWindowOpenHandler\(\(\) => \(\{ action: 'deny' \}\)\)" -Label "new windows blocked"
Assert-RequiredText -Text $mainText -Pattern "will-navigate" -Label "navigation guard present"
Assert-RequiredText -Text $mainText -Pattern "assertTrustedSender" -Label "desktop IPC sender guard"
Assert-RequiredText -Text $preloadText -Pattern "contextBridge\.exposeInMainWorld" -Label "preload exposes controlled bridge"
Assert-RequiredText -Text $preloadText -Pattern "milu-api-base" -Label "preload injects Control API base URL"
Assert-RequiredText -Text $preloadText -Pattern "milu-desktop-token" -Label "preload injects desktop session token"
Assert-ForbiddenText -Text $preloadText -Pattern "require\(|fs\.|child_process|sqlite|postgres" -Label "preload has no direct runtime/database/media access"
Assert-RequiredText -Text $webHostText -Pattern "Content-Security-Policy" -Label "desktop web host sets CSP"
Assert-RequiredText -Text $webHostText -Pattern "default-src 'self'" -Label "CSP default source is self"
Assert-RequiredText -Text $webHostText -Pattern "script-src 'self'" -Label "CSP script source is self"
Assert-RequiredText -Text $webHostText -Pattern "connect-src 'self' http://127\.0\.0\.1:\* http://localhost:\*" -Label "CSP connect source stays loopback"
Assert-RequiredText -Text $webHostText -Pattern "object-src 'none'" -Label "CSP blocks object embedding"
Assert-RequiredText -Text $webHostText -Pattern "base-uri 'self'" -Label "CSP limits base URI"
Assert-RequiredText -Text $webHostText -Pattern "form-action 'self'" -Label "CSP limits form actions"
Assert-RequiredText -Text $webHostText -Pattern "frame-ancestors 'none'" -Label "CSP blocks framing"
Assert-RequiredText -Text $webHostText -Pattern "X-Content-Type-Options', 'nosniff'" -Label "desktop web host sets nosniff"

Assert-RequiredText -Text $pathsText -Pattern "path\.dirname\(process\.execPath\)" -Label "packaged data is rooted beside installed exe"
Assert-RequiredText -Text $pathsText -Pattern "path\.join\(dataRoot, 'storage'\)" -Label "storage stays under desktop data root"
Assert-RequiredText -Text $pathsText -Pattern "path\.join\(dataRoot, 'logs', 'desktop'\)" -Label "logs stay under desktop data root"
Assert-RequiredText -Text $pathsText -Pattern "path\.join\(dataRoot, 'outputs'\)" -Label "outputs stay under desktop data root"
Assert-RequiredText -Text $pathsText -Pattern "path\.join\(controlPlaneRoot, 'db', 'sqlite'\)" -Label "SQLite metadata root bundled as API resource only"

Assert-RequiredText -Text $installerScriptText -Pattern "!macro customWelcomePage" -Label "installer custom integration page"
Assert-RequiredText -Text $installerScriptText -Pattern "DesktopShortcutCheckbox" -Label "desktop shortcut option"
Assert-RequiredText -Text $installerScriptText -Pattern "StartMenuShortcutCheckbox" -Label "Start Menu shortcut option"
Assert-RequiredText -Text $installerScriptText -Pattern '\$SMSTARTUP\\MiLuStudio\.lnk' -Label "startup shortcut action"
Assert-ForbiddenText -Text $installerScriptText -Pattern "license|activation|payment|paid|License|Activation|Payment|Paid" -Label "installer has no license/payment gate"

Assert-RequiredFile -Path $InstallerPath -Label "Windows installer artifact" -MinimumBytes $MinimumInstallerBytes
Assert-RequiredFile -Path "$InstallerPath.blockmap" -Label "installer blockmap" -MinimumBytes 1
Assert-RequiredFile -Path (Join-Path $UnpackedRoot "MiLuStudio.exe") -Label "unpacked desktop executable" -MinimumBytes 1
Assert-RequiredFile -Path (Join-Path $resourcesRoot "app.asar") -Label "packaged app.asar" -MinimumBytes 1
Assert-RequiredFile -Path (Join-Path $resourcesRoot "build\icon.ico") -Label "packaged app icon" -MinimumBytes 1
Assert-RequiredFile -Path (Join-Path $resourcesRoot "web\index.html") -Label "packaged Web dist" -MinimumBytes 1
Assert-RequiredFile -Path (Join-Path $resourcesRoot "web\brand\logo.png") -Label "packaged Web brand logo" -MinimumBytes 1
Assert-AnyFile -Paths @(
    (Join-Path $resourcesRoot "control-plane\api\MiLuStudio.Api.exe"),
    (Join-Path $resourcesRoot "control-plane\api\MiLuStudio.Api.dll")
) -Label "packaged Control API runtime"
Assert-AnyFile -Paths @(
    (Join-Path $resourcesRoot "control-plane\worker\MiLuStudio.Worker.exe"),
    (Join-Path $resourcesRoot "control-plane\worker\MiLuStudio.Worker.dll")
) -Label "packaged Worker runtime"
Assert-RequiredDirectory -Path (Join-Path $resourcesRoot "control-plane\db\sqlite") -Label "packaged SQLite metadata root"
Assert-RequiredFile -Path (Join-Path $resourcesRoot "control-plane\db\sqlite\README.txt") -Label "packaged SQLite metadata marker" -MinimumBytes 1
Assert-RequiredFile -Path (Join-Path $resourcesRoot "python-runtime\python.exe") -Label "packaged Python runtime" -MinimumBytes 1
Assert-RequiredFile -Path (Join-Path $resourcesRoot "python-skills\milu_studio_skills\gateway.py") -Label "packaged Python skill gateway" -MinimumBytes 1
Assert-RequiredFile -Path (Join-Path $resourcesRoot "python-skills\skills\storyboard_director\skill.yaml") -Label "packaged storyboard director skill" -MinimumBytes 1

$signingReports = @(
    (Test-Signature -Path $InstallerPath -Label "installer"),
    (Test-Signature -Path (Join-Path $UnpackedRoot "MiLuStudio.exe") -Label "desktop executable")
)

$certPath = $env:MILUSTUDIO_CODESIGN_CERT_PATH
$certThumbprint = $env:MILUSTUDIO_CODESIGN_CERT_THUMBPRINT
$timestampUrl = $env:MILUSTUDIO_CODESIGN_TIMESTAMP_URL
$hasCertPath = -not [string]::IsNullOrWhiteSpace($certPath)
$hasThumbprint = -not [string]::IsNullOrWhiteSpace($certThumbprint)

if ($hasCertPath) {
    $fullCertPath = [System.IO.Path]::GetFullPath($certPath)
    if ($fullCertPath.StartsWith($ProjectRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Code signing certificate path must not be inside the repository: $fullCertPath"
    }

    if (-not (Test-Path -LiteralPath $fullCertPath -PathType Leaf)) {
        throw "Code signing certificate path does not exist: $fullCertPath"
    }

    Write-Stage19Pass -Label "code signing certificate path is outside repository"
}

if (-not ($hasCertPath -or $hasThumbprint)) {
    Write-Stage19Warning "No MILUSTUDIO_CODESIGN_CERT_PATH or MILUSTUDIO_CODESIGN_CERT_THUMBPRINT is configured. Formal signing remains blocked."
}
else {
    Write-Stage19Pass -Label "code signing certificate selector configured"
}

if ([string]::IsNullOrWhiteSpace($timestampUrl)) {
    Write-Stage19Warning "No MILUSTUDIO_CODESIGN_TIMESTAMP_URL is configured. Formal signing should use a trusted timestamp server."
}
else {
    Assert-Condition -Condition ($timestampUrl -match '^https?://') -Message "MILUSTUDIO_CODESIGN_TIMESTAMP_URL must be an http(s) URL." -Label "timestamp server URL format"
}

if (-not $SkipApiSecurity) {
    powershell -ExecutionPolicy Bypass -File (Join-Path $ProjectRoot "scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1") `
        -ProjectRoot $ProjectRoot `
        -NpmPath $NpmPath `
        -SkipPrepareRuntime
    if ($LASTEXITCODE -ne 0) {
        throw "Desktop Control API boundary verification failed."
    }

    Write-Stage19Pass -Label "desktop Control API boundary verification"
}

$report = [pscustomobject]@{
    stage = "Stage 19"
    generatedAt = (Get-Date).ToString("o")
    projectRoot = $ProjectRoot
    installerPath = $InstallerPath
    unpackedRoot = $UnpackedRoot
    buildPackage = [bool]$BuildPackage
    requireSigned = [bool]$RequireSigned
    checks = @($checks)
    warnings = @($warnings)
    signatures = $signingReports
    signingPreflight = [pscustomobject]@{
        hasCertificatePath = $hasCertPath
        hasCertificateThumbprint = $hasThumbprint
        hasTimestampUrl = -not [string]::IsNullOrWhiteSpace($timestampUrl)
    }
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $reportPath) | Out-Null
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportPath -Encoding UTF8

Write-Host "MiLuStudio Stage 19 desktop release verification passed."
Write-Host "Report: $reportPath"
