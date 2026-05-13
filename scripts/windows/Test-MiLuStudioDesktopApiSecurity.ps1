param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$NpmPath = "D:\soft\program\nodejs\npm.ps1",
    [int]$Port = 55368,
    [string]$AllowedOrigin = "http://127.0.0.1:55999",
    [string]$DesktopSessionToken = "stage15-desktop-api-test",
    [switch]$SkipPrepareRuntime
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$desktopRoot = Join-Path $ProjectRoot "apps\desktop"
$apiRoot = Join-Path $desktopRoot "runtime\control-plane\api"
$apiExe = Join-Path $apiRoot "MiLuStudio.Api.exe"
$apiBaseUrl = "http://127.0.0.1:$Port"
$connectionString = if ($env:ConnectionStrings__MiLuStudioControlPlane) {
    $env:ConnectionStrings__MiLuStudioControlPlane
}
else {
    "Host=127.0.0.1;Port=5432;Database=milu;Username=root;Password=root"
}

function Invoke-HttpStatus {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )

    try {
        if ([string]::IsNullOrEmpty($Body)) {
            Invoke-RestMethod -Uri $Uri -Method $Method -Headers $Headers -ErrorAction Stop | Out-Null
        }
        else {
            Invoke-RestMethod -Uri $Uri -Method $Method -Headers $Headers -ContentType "application/json" -Body $Body -ErrorAction Stop | Out-Null
        }

        return 200
    }
    catch {
        if ($null -eq $_.Exception.Response) {
            return 0
        }

        return [int]$_.Exception.Response.StatusCode
    }
}

function Assert-Status {
    param(
        [int]$Actual,
        [int]$Expected,
        [string]$Label
    )

    if ($Actual -ne $Expected) {
        throw "$Label expected HTTP $Expected but received HTTP $Actual."
    }
}

if (-not $SkipPrepareRuntime) {
    Push-Location $desktopRoot
    try {
        & $NpmPath run prepare:runtime
        if ($LASTEXITCODE -ne 0) {
            throw "Desktop runtime preparation failed."
        }
    }
    finally {
        Pop-Location
    }
}

if (-not (Test-Path -LiteralPath $apiExe)) {
    throw "Control API desktop executable not found: $apiExe"
}

$processInfo = [System.Diagnostics.ProcessStartInfo]::new()
$processInfo.FileName = $apiExe
$processInfo.WorkingDirectory = $apiRoot
$processInfo.UseShellExecute = $false
$processInfo.CreateNoWindow = $true
$processInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Production"
$processInfo.EnvironmentVariables["DOTNET_ENVIRONMENT"] = "Production"
$processInfo.EnvironmentVariables["ASPNETCORE_URLS"] = $apiBaseUrl
$processInfo.EnvironmentVariables["ConnectionStrings__MiLuStudioControlPlane"] = $connectionString
$processInfo.EnvironmentVariables["ControlPlane__RepositoryProvider"] = "PostgreSQL"
$processInfo.EnvironmentVariables["ControlPlane__MigrationsPath"] = Join-Path $desktopRoot "runtime\control-plane\db\migrations"
$processInfo.EnvironmentVariables["ControlPlane__DesktopMode"] = "true"
$processInfo.EnvironmentVariables["ControlPlane__AllowedDesktopOrigin"] = $AllowedOrigin
$processInfo.EnvironmentVariables["ControlPlane__DesktopSessionToken"] = $DesktopSessionToken
$processInfo.EnvironmentVariables["ControlPlane__StorageRoot"] = Join-Path $ProjectRoot "storage"
$processInfo.EnvironmentVariables["ControlPlane__PythonExecutablePath"] = Join-Path $desktopRoot "runtime\python-runtime\python.exe"
$processInfo.EnvironmentVariables["ControlPlane__PythonSkillsRoot"] = Join-Path $desktopRoot "runtime\python-skills"
$processInfo.EnvironmentVariables["ControlPlane__SkillRunTempRoot"] = Join-Path $ProjectRoot ".tmp\skill-runs"

$apiProcess = [System.Diagnostics.Process]::Start($processInfo)
try {
    $deadline = (Get-Date).AddSeconds(45)
    do {
        Start-Sleep -Milliseconds 500
        $healthStatus = Invoke-HttpStatus -Uri "$apiBaseUrl/health"
        if ($healthStatus -eq 200) {
            break
        }
    } while ((Get-Date) -lt $deadline)

    Assert-Status -Actual $healthStatus -Expected 200 -Label "Control API health"

    $withoutToken = Invoke-HttpStatus -Uri "$apiBaseUrl/api/projects" -Method "POST" -Body "{}"
    Assert-Status -Actual $withoutToken -Expected 403 -Label "Unsafe desktop request without token"

    $withToken = Invoke-HttpStatus `
        -Uri "$apiBaseUrl/api/projects" `
        -Method "POST" `
        -Headers @{ "X-MiLuStudio-Desktop-Token" = $DesktopSessionToken } `
        -Body "{}"
    Assert-Status -Actual $withToken -Expected 401 -Label "Unsafe desktop request with token reaches auth gate"

    $migrationApply = Invoke-HttpStatus `
        -Uri "$apiBaseUrl/api/system/migrations/apply" `
        -Method "POST" `
        -Headers @{ "X-MiLuStudio-Desktop-Token" = $DesktopSessionToken } `
        -Body "{}"
    Assert-Status -Actual $migrationApply -Expected 403 -Label "Desktop migration apply"

    Write-Host "MiLuStudio desktop API security verification passed."
}
finally {
    if ($apiProcess -and -not $apiProcess.HasExited) {
        $apiProcess.Kill()
        $apiProcess.WaitForExit(5000) | Out-Null
    }
}
