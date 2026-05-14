param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5378",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage18-provider-build"
$solution = Join-Path $ProjectRoot "backend\control-plane\MiLuStudio.ControlPlane.sln"
$apiDll = Join-Path $buildOutput "MiLuStudio.Api.dll"
$settingsRoot = Join-Path $ProjectRoot (".tmp\stage18-provider-settings\" + ([guid]::NewGuid().ToString("N")))
$settingsPath = Join-Path $settingsRoot "provider-adapters.local.json"
$startedProcesses = New-Object System.Collections.Generic.List[System.Diagnostics.Process]
$script:AuthHeaders = @{}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null
    )

    $uri = "$ApiBaseUrl$Path"
    try {
        if ($null -eq $Body) {
            return Invoke-RestMethod -Method $Method -Uri $uri -Headers $script:AuthHeaders
        }

        return Invoke-RestMethod `
            -Method $Method `
            -Uri $uri `
            -Headers $script:AuthHeaders `
            -ContentType "application/json; charset=utf-8" `
            -Body ($Body | ConvertTo-Json -Depth 80)
    }
    catch {
        $detail = ""
        if ($_.Exception.Response -and $_.Exception.Response.GetResponseStream()) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $detail = $reader.ReadToEnd()
        }

        throw "API $Method $Path failed: $($_.Exception.Message) $detail"
    }
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
    $env:ControlPlane__RepositoryProvider = "InMemory"
    $env:ControlPlane__StorageRoot = $settingsRoot
    $env:ControlPlane__ProviderSettingsPath = $settingsPath

    $process = Start-Process -FilePath $DotnetPath -ArgumentList @($apiDll) -WorkingDirectory $buildOutput -WindowStyle Hidden -PassThru
    $startedProcesses.Add($process)
    Wait-ApiHealthy
    if ($process.HasExited) {
        throw "Control API process exited early. Another process may already be bound to $ApiBaseUrl."
    }
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
        Where-Object {
            $_.CommandLine -like "*$buildOutput*" -or
            ($_.CommandLine -like "*$ProjectRoot*" -and $_.CommandLine -like "*MiLuStudio.Api.dll*")
        } |
        ForEach-Object {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        }

    Start-Sleep -Milliseconds 500
}

try {
    Stop-IntegrationBuildProcesses

    if (-not $SkipBuild) {
        & $DotnetPath build $solution --no-restore "-p:OutputPath=$buildOutput\"
        if ($LASTEXITCODE -ne 0) {
            throw ".NET build failed."
        }
    }

    $api = Start-ControlApi

    $authSuffix = ([guid]::NewGuid().ToString("N")).Substring(0, 12)
    $authSession = Invoke-Api -Method Post -Path "/api/auth/register" -Body @{
        email = "stage18_$authSuffix@example.local"
        displayName = "Stage 18 Provider Settings"
        password = "Stage18-Test-Password!"
        deviceFingerprint = "stage18-device-$authSuffix"
        deviceName = "Stage 18 PowerShell Device"
    }
    $script:AuthHeaders = @{ Authorization = "Bearer $($authSession.accessToken)" }

    $initial = Invoke-Api -Method Get -Path "/api/settings/providers"
    if ($initial.mode -ne "placeholder_only") {
        throw "Provider settings mode is not placeholder_only."
    }

    $secret = "stage18-secret-key-should-not-be-written"
    $adapterUpdates = @($initial.adapters | ForEach-Object {
        $supplier = $_.supplier
        $enabled = $_.enabled
        $apiKey = $null
        if ($_.kind -eq "text") {
            $supplier = "openai"
            $enabled = $true
            $apiKey = $secret
        }

        @{
            kind = $_.kind
            supplier = $supplier
            model = $_.model
            enabled = $enabled
            apiKey = $apiKey
            clearApiKey = $false
        }
    })

    $updated = Invoke-Api -Method Patch -Path "/api/settings/providers" -Body @{
        costGuardrails = @{
            projectCostCapCny = 88.5
            retryLimit = 2
        }
        adapters = $adapterUpdates
    }

    $updatedJson = $updated | ConvertTo-Json -Depth 80
    if ($updatedJson -like "*$secret*") {
        throw "Provider settings response leaked the raw API key."
    }

    $textAdapter = $updated.adapters | Where-Object { $_.kind -eq "text" } | Select-Object -First 1
    if (-not $textAdapter.apiKeyConfigured -or $textAdapter.apiKeyPreview -notlike "****tten") {
        throw "Text adapter did not persist masked API key metadata."
    }

    if (-not (Test-Path -LiteralPath $settingsPath)) {
        throw "Provider settings file was not created."
    }

    $settingsFile = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8
    if ($settingsFile -like "*$secret*") {
        throw "Provider settings file leaked the raw API key."
    }

    $preflight = Invoke-Api -Method Get -Path "/api/settings/providers/preflight"
    $textCheck = $preflight.checks | Where-Object { $_.kind -eq "text" } | Select-Object -First 1
    if ($textCheck.status -ne "ok" -or $textCheck.details.externalNetwork -ne "disabled" -or $textCheck.details.mediaGenerated -ne "false") {
        throw "Provider preflight crossed the placeholder/no-media boundary."
    }

    $clearUpdates = @($updated.adapters | ForEach-Object {
        @{
            kind = $_.kind
            supplier = $_.supplier
            model = $_.model
            enabled = $_.enabled
            apiKey = $null
            clearApiKey = $_.kind -eq "text"
        }
    })
    $cleared = Invoke-Api -Method Patch -Path "/api/settings/providers" -Body @{
        costGuardrails = $updated.costGuardrails
        adapters = $clearUpdates
    }
    $clearedText = $cleared.adapters | Where-Object { $_.kind -eq "text" } | Select-Object -First 1
    if ($clearedText.apiKeyConfigured) {
        throw "Clear API key did not remove placeholder secret metadata."
    }

    Write-Host "Stage 18 provider settings passed. Settings file: $settingsPath"
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }

    Stop-IntegrationBuildProcesses
}
