param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5368",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [int]$TimeoutSeconds = 90,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage16-auth-build"
$solution = Join-Path $ProjectRoot "backend\control-plane\MiLuStudio.ControlPlane.sln"
$apiDll = Join-Path $buildOutput "MiLuStudio.Api.dll"
$startedProcesses = New-Object System.Collections.Generic.List[System.Diagnostics.Process]

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )

    $uri = "$ApiBaseUrl$Path"
    if ($null -eq $Body) {
        return Invoke-RestMethod -Method $Method -Uri $uri -Headers $Headers
    }

    return Invoke-RestMethod `
        -Method $Method `
        -Uri $uri `
        -Headers $Headers `
        -ContentType "application/json; charset=utf-8" `
        -Body ($Body | ConvertTo-Json -Depth 20)
}

function Invoke-HttpStatus {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )

    try {
        Invoke-Api -Method $Method -Path $Path -Body $Body -Headers $Headers | Out-Null
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

function New-AuthHeader {
    param([string]$Token)
    return @{ Authorization = "Bearer $Token" }
}

function Wait-ApiHealthy {
    $deadline = (Get-Date).AddSeconds(45)
    do {
        try {
            $health = Invoke-Api -Method Get -Path "/health"
            if ($health.status -eq "ok") {
                return
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    } while ((Get-Date) -lt $deadline)

    throw "Control API did not become healthy at $ApiBaseUrl."
}

function Start-ControlApi {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = $ApiBaseUrl
    $process = Start-Process -FilePath $DotnetPath -ArgumentList @($apiDll) -WorkingDirectory $buildOutput -WindowStyle Hidden -PassThru
    $startedProcesses.Add($process)
    Wait-ApiHealthy
    return $process
}

function Stop-StartedProcess {
    param([System.Diagnostics.Process]$Process)

    if ($null -eq $Process -or $Process.HasExited) {
        return
    }

    Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
    try {
        $Process.WaitForExit(5000) | Out-Null
    }
    catch {
        Start-Sleep -Milliseconds 500
    }
}

function Stop-IntegrationBuildProcesses {
    Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { $_.CommandLine -like "*$buildOutput*" } |
        ForEach-Object {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        }

    Start-Sleep -Milliseconds 500
}

try {
    Stop-IntegrationBuildProcesses
    powershell -ExecutionPolicy Bypass -File (Join-Path $ProjectRoot "scripts\windows\Initialize-MiLuStudioPostgreSql.ps1")

    if (-not $SkipBuild) {
        & $DotnetPath build $solution --no-restore "-p:OutputPath=$buildOutput\"
        if ($LASTEXITCODE -ne 0) {
            throw ".NET build failed."
        }
    }

    $api = Start-ControlApi
    Invoke-Api -Method Post -Path "/api/system/migrations/apply" | Out-Null
    $migrationStatus = Invoke-Api -Method Get -Path "/api/system/migrations"
    if ($migrationStatus.status -ne "up_to_date") {
        throw "Migrations are not up to date."
    }

    $unauthProjects = Invoke-HttpStatus -Method Get -Path "/api/projects"
    Assert-Status -Actual $unauthProjects -Expected 401 -Label "Unauthenticated projects request"

    $suffix = ([guid]::NewGuid().ToString("N")).Substring(0, 12)
    $email = "stage16_$suffix@example.local"
    $password = "Stage16-Test-Password!"
    $deviceFingerprint = "stage16-device-$suffix"
    $deviceName = "Stage16 PowerShell Device"

    $registered = Invoke-Api -Method Post -Path "/api/auth/register" -Body @{
        email = $email
        displayName = "Stage16 Auth Test"
        password = $password
        deviceFingerprint = $deviceFingerprint
        deviceName = $deviceName
    }
    if (-not $registered.accessToken -or $registered.license.isActive) {
        throw "Registered account should have a session and should still require license activation."
    }

    $authHeader = New-AuthHeader -Token $registered.accessToken
    $licenseBeforeActivation = Invoke-Api -Method Get -Path "/api/auth/license" -Headers $authHeader
    if ($licenseBeforeActivation.status -ne "missing") {
        throw "New account should report a missing license before activation."
    }

    $licensedProjectsBeforeActivation = Invoke-HttpStatus -Method Get -Path "/api/projects" -Headers $authHeader
    Assert-Status -Actual $licensedProjectsBeforeActivation -Expected 403 -Label "Unlicensed projects request"

    $invalidActivation = Invoke-HttpStatus -Method Post -Path "/api/auth/activate" -Headers $authHeader -Body @{
        activationCode = "BAD-STAGE16-CODE"
        deviceFingerprint = $deviceFingerprint
        deviceName = $deviceName
    }
    Assert-Status -Actual $invalidActivation -Expected 403 -Label "Invalid activation code"

    $activated = Invoke-Api -Method Post -Path "/api/auth/activate" -Headers $authHeader -Body @{
        activationCode = "MILU-STAGE16-TEST"
        deviceFingerprint = $deviceFingerprint
        deviceName = $deviceName
    }
    if (-not $activated.license.isActive) {
        throw "License activation did not produce an active license."
    }

    $projectsAfterActivation = Invoke-HttpStatus -Method Get -Path "/api/projects" -Headers $authHeader
    Assert-Status -Actual $projectsAfterActivation -Expected 200 -Label "Licensed projects request"

    $refreshed = Invoke-Api -Method Post -Path "/api/auth/refresh" -Body @{
        refreshToken = $registered.refreshToken
        deviceFingerprint = $deviceFingerprint
        deviceName = $deviceName
    }
    if (-not $refreshed.accessToken -or $refreshed.accessToken -eq $registered.accessToken) {
        throw "Session refresh did not rotate the access token."
    }

    $refreshHeader = New-AuthHeader -Token $refreshed.accessToken
    $bound = Invoke-Api -Method Post -Path "/api/auth/devices/bind" -Headers $refreshHeader -Body @{
        deviceFingerprint = "$deviceFingerprint-secondary"
        deviceName = "Stage16 Secondary Device"
    }
    if (-not $bound.device.trusted) {
        throw "Device binding did not return a trusted Control API-managed device."
    }

    $thirdDevice = Invoke-HttpStatus -Method Post -Path "/api/auth/devices/bind" -Headers $refreshHeader -Body @{
        deviceFingerprint = "$deviceFingerprint-third"
        deviceName = "Stage16 Third Device"
    }
    Assert-Status -Actual $thirdDevice -Expected 403 -Label "Device limit request"

    Invoke-Api -Method Post -Path "/api/auth/logout" -Headers $refreshHeader -Body @{
        refreshToken = $refreshed.refreshToken
    } | Out-Null
    $afterLogout = Invoke-HttpStatus -Method Get -Path "/api/projects" -Headers $refreshHeader
    Assert-Status -Actual $afterLogout -Expected 401 -Label "Projects request after logout"

    $loggedIn = Invoke-Api -Method Post -Path "/api/auth/login" -Body @{
        identifier = $email
        password = $password
        deviceFingerprint = $deviceFingerprint
        deviceName = $deviceName
    }
    if (-not $loggedIn.license.isActive) {
        throw "Login should restore active license state for the account."
    }

    $loginHeader = New-AuthHeader -Token $loggedIn.accessToken
    $licensedProjectsAfterLogin = Invoke-HttpStatus -Method Get -Path "/api/projects" -Headers $loginHeader
    Assert-Status -Actual $licensedProjectsAfterLogin -Expected 200 -Label "Licensed projects request after login"

    Write-Host "Stage 16 auth/licensing integration passed for account $email."
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }

    Stop-IntegrationBuildProcesses
}
