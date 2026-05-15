param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [int]$ApiPort = 5368,
    [switch]$SkipBuild,
    [switch]$NoWorker
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$serviceRoot = Join-Path $ProjectRoot ".tmp\dev-services"
$buildOutput = Join-Path $serviceRoot "build"
$logRoot = Join-Path $serviceRoot "logs"
$stateFile = Join-Path $serviceRoot "services.json"
$sqlitePath = Join-Path $ProjectRoot "storage\milu-control-plane.dev.sqlite3"
$apiBaseUrl = "http://127.0.0.1:$ApiPort"
$solution = Join-Path $ProjectRoot "backend\control-plane\MiLuStudio.ControlPlane.sln"
$apiDll = Join-Path $buildOutput "MiLuStudio.Api.dll"
$workerDll = Join-Path $buildOutput "MiLuStudio.Worker.dll"

New-Item -ItemType Directory -Force -Path $serviceRoot, $buildOutput, $logRoot, (Split-Path -Parent $sqlitePath) | Out-Null

if (-not (Test-Path -LiteralPath $DotnetPath)) {
    throw "dotnet runtime not found: $DotnetPath"
}

function Test-ApiHealth {
    param([string]$Url)

    try {
        $health = Invoke-RestMethod -Uri "$Url/health" -TimeoutSec 2
        return [bool]($health.service -eq "MiLuStudio Control API")
    }
    catch {
        return $false
    }
}

function Test-RecordedProcessAlive {
    if (-not (Test-Path -LiteralPath $stateFile)) {
        return $false
    }

    try {
        $state = Get-Content -LiteralPath $stateFile -Raw | ConvertFrom-Json
        foreach ($service in $state.services) {
            if ($service.pid -and (Get-Process -Id $service.pid -ErrorAction SilentlyContinue)) {
                return $true
            }
        }
    }
    catch {
        return $false
    }

    return $false
}

function Wait-ApiHealthy {
    $deadline = (Get-Date).AddSeconds(45)
    do {
        if (Test-ApiHealth -Url $apiBaseUrl) {
            return
        }

        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    throw "Control API did not become healthy at $apiBaseUrl."
}

if (Test-ApiHealth -Url $apiBaseUrl) {
    Write-Host "MiLuStudio Control API is already reachable at $apiBaseUrl."
    Write-Host "Web dev can use the existing service. Run Stop-MiLuStudioLocalServices.ps1 only for services started by this script."
    exit 0
}

$portOwner = Get-NetTCPConnection -LocalAddress 127.0.0.1 -LocalPort $ApiPort -State Listen -ErrorAction SilentlyContinue
if ($portOwner) {
    throw "Port $ApiPort is already in use but does not respond as MiLuStudio Control API."
}

if (Test-RecordedProcessAlive) {
    Write-Host "Recorded MiLuStudio local service state exists, but Control API health check failed. Clearing stale services before restart."
    & (Join-Path $ProjectRoot "scripts\windows\Stop-MiLuStudioLocalServices.ps1") -ProjectRoot $ProjectRoot
}

if (-not $SkipBuild) {
    & $DotnetPath build $solution "-p:OutputPath=$buildOutput\"
    if ($LASTEXITCODE -ne 0) {
        throw "Control Plane build failed."
    }
}

if (-not (Test-Path -LiteralPath $apiDll)) {
    throw "API runtime not found: $apiDll"
}

if (-not $NoWorker -and -not (Test-Path -LiteralPath $workerDll)) {
    throw "Worker runtime not found: $workerDll"
}

$commonEnv = @{
    "ConnectionStrings__MiLuStudioControlPlane" = "Data Source=$sqlitePath"
    "ControlPlane__RepositoryProvider" = "SQLite"
    "ControlPlane__MigrationsPath" = (Join-Path $ProjectRoot "backend\control-plane\db\sqlite")
    "ControlPlane__StorageRoot" = (Join-Path $ProjectRoot "storage")
    "ControlPlane__UploadsRoot" = (Join-Path $ProjectRoot "uploads")
    "ControlPlane__TempRoot" = (Join-Path $ProjectRoot ".tmp")
    "ControlPlane__PythonExecutablePath" = $env:MILUSTUDIO_PYTHON
    "ControlPlane__PythonSkillsRoot" = (Join-Path $ProjectRoot "backend\sidecars\python-skills")
    "ControlPlane__SkillRunTempRoot" = (Join-Path $ProjectRoot ".tmp\skill-runs")
}

foreach ($entry in $commonEnv.GetEnumerator()) {
    [System.Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, "Process")
}

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = $apiBaseUrl
$env:ControlPlane__WorkerId = "milu-dev-api"

$apiProcess = $null
$workerProcess = $null

try {
    $apiProcess = Start-Process `
        -FilePath $DotnetPath `
        -ArgumentList @($apiDll) `
        -WorkingDirectory $buildOutput `
        -RedirectStandardOutput (Join-Path $logRoot "control-api.out.log") `
        -RedirectStandardError (Join-Path $logRoot "control-api.err.log") `
        -WindowStyle Hidden `
        -PassThru

    Wait-ApiHealthy
    Invoke-RestMethod -Method Post -Uri "$apiBaseUrl/api/system/migrations/apply" -TimeoutSec 15 | Out-Null

    $services = @(
        [ordered]@{
            name = "controlApi"
            pid = $apiProcess.Id
            expectedCommand = "MiLuStudio.Api.dll"
        }
    )

    if (-not $NoWorker) {
        $env:DOTNET_ENVIRONMENT = "Development"
        $env:ControlPlane__WorkerId = "milu-dev-worker"

        $workerProcess = Start-Process `
            -FilePath $DotnetPath `
            -ArgumentList @($workerDll) `
            -WorkingDirectory $buildOutput `
            -RedirectStandardOutput (Join-Path $logRoot "worker.out.log") `
            -RedirectStandardError (Join-Path $logRoot "worker.err.log") `
            -WindowStyle Hidden `
            -PassThru

        $services += [ordered]@{
            name = "worker"
            pid = $workerProcess.Id
            expectedCommand = "MiLuStudio.Worker.dll"
        }
    }

    [ordered]@{
        apiBaseUrl = $apiBaseUrl
        sqlitePath = $sqlitePath
        buildOutput = $buildOutput
        logRoot = $logRoot
        startedAt = (Get-Date).ToString("o")
        services = $services
    } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $stateFile -Encoding UTF8

    Write-Host "MiLuStudio local services are running."
    Write-Host "Control API: $apiBaseUrl"
    Write-Host "SQLite: $sqlitePath"
    Write-Host "Logs: $logRoot"
    Write-Host "Stop with: powershell -ExecutionPolicy Bypass -File $ProjectRoot\scripts\windows\Stop-MiLuStudioLocalServices.ps1"
}
catch {
    if ($workerProcess -and -not $workerProcess.HasExited) {
        Stop-Process -Id $workerProcess.Id -Force -ErrorAction SilentlyContinue
    }
    if ($apiProcess -and -not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }

    throw
}
