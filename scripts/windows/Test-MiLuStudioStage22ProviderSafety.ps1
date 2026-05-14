param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5388",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage22-provider-safety-build"
$solution = Join-Path $ProjectRoot "backend\control-plane\MiLuStudio.ControlPlane.sln"
$apiDll = Join-Path $buildOutput "MiLuStudio.Api.dll"
$settingsRoot = Join-Path $ProjectRoot (".tmp\stage22-provider-safety\" + ([guid]::NewGuid().ToString("N")))
$settingsPath = Join-Path $settingsRoot "provider-adapters.local.json"
$secretStorePath = Join-Path $settingsRoot "provider-secrets.local.json"
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
    $env:ControlPlane__ProviderSecretStorePath = $secretStorePath

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

function Assert-NoRawSecret {
    param(
        [string]$Value,
        [string]$Secret,
        [string]$Name
    )

    if ($Value -like "*$Secret*") {
        throw "$Name leaked the raw provider secret."
    }
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
        email = "stage22_$authSuffix@example.local"
        displayName = "Stage 22 Provider Safety"
        password = "Stage22-Test-Password!"
        deviceFingerprint = "stage22-device-$authSuffix"
        deviceName = "Stage 22 PowerShell Device"
    }
    $script:AuthHeaders = @{ Authorization = "Bearer $($authSession.accessToken)" }

    $initial = Invoke-Api -Method Get -Path "/api/settings/providers"
    if ($initial.mode -ne "placeholder_only" -or $initial.safety.stage -ne "stage22_provider_safety_preflight") {
        throw "Provider settings did not expose Stage 22 safety posture."
    }
    if ($initial.safety.sandbox.providerCallsAllowed -ne $false -or $initial.safety.sandbox.externalNetworkAllowed -ne $false) {
        throw "Initial provider sandbox crossed the no-provider/no-network boundary."
    }

    $secret = "stage22-secret-key-should-not-be-written"
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
            projectCostCapCny = 12.25
            retryLimit = 1
        }
        adapters = $adapterUpdates
    }

    Assert-NoRawSecret -Value ($updated | ConvertTo-Json -Depth 80) -Secret $secret -Name "Provider settings response"

    $textAdapter = $updated.adapters | Where-Object { $_.kind -eq "text" } | Select-Object -First 1
    if (-not $textAdapter.apiKeyConfigured -or $textAdapter.apiKeyPreview -notlike "****tten") {
        throw "Text adapter did not persist masked key metadata."
    }
    if ($textAdapter.safety.rawSecretPersisted -ne $false -or $textAdapter.safety.usableForProviderCalls -ne $false) {
        throw "Text adapter secret safety crossed the Stage 22 metadata-only boundary."
    }
    if ($textAdapter.safety.providerCallsAllowed -ne $false -or $textAdapter.safety.externalNetworkAllowed -ne $false -or $textAdapter.safety.ffmpegAllowed -ne $false) {
        throw "Text adapter sandbox allowed a forbidden capability."
    }

    if (-not (Test-Path -LiteralPath $settingsPath)) {
        throw "Provider settings file was not created."
    }
    if (-not (Test-Path -LiteralPath $secretStorePath)) {
        throw "Provider secret metadata store was not created."
    }

    $settingsFile = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8
    $secretStoreFile = Get-Content -LiteralPath $secretStorePath -Raw -Encoding UTF8
    Assert-NoRawSecret -Value $settingsFile -Secret $secret -Name "Provider settings file"
    Assert-NoRawSecret -Value $secretStoreFile -Secret $secret -Name "Provider secret store file"
    if ($secretStoreFile -notlike "*stage22_metadata_only*" -or $secretStoreFile -like "*usableForProviderCalls`": true*") {
        throw "Provider secret store did not record metadata-only, non-callable secret posture."
    }

    $safety = Invoke-Api -Method Get -Path "/api/settings/providers/safety"
    if ($safety.secretStore.rawSecretPersistenceAllowed -ne $false -or $safety.secretStore.providerCallSecretsAvailable -ne $false) {
        throw "Safety endpoint exposed raw or callable provider secrets."
    }
    if ($safety.spendGuard.enabled -ne $true -or $safety.spendGuard.blocksProviderCalls -ne $true) {
        throw "Spend guard is not enforcing Stage 22 provider call blocking."
    }
    if ($safety.sandbox.mediaReadAllowed -ne $false -or $safety.sandbox.ffmpegAllowed -ne $false) {
        throw "Sandbox crossed the no-media/no-FFmpeg boundary."
    }

    $preflight = Invoke-Api -Method Get -Path "/api/settings/providers/preflight"
    foreach ($kind in @("secret_store", "spend_guard", "provider_sandbox", "text")) {
        $check = $preflight.checks | Where-Object { $_.kind -eq $kind } | Select-Object -First 1
        if ($null -eq $check) {
            throw "Preflight did not include $kind."
        }
        if ($kind -ne "text" -and $check.status -ne "ok") {
            throw "Preflight safety check $kind was not ok."
        }
    }
    $textCheck = $preflight.checks | Where-Object { $_.kind -eq "text" } | Select-Object -First 1
    if ($textCheck.status -ne "ok" -or $textCheck.details.providerCalls -ne "blocked" -or $textCheck.details.ffmpegInvoked -ne "false") {
        throw "Text preflight crossed Stage 22 safety boundaries."
    }

    $withinBudget = Invoke-Api -Method Post -Path "/api/settings/providers/spend-guard/check" -Body @{
        projectId = "stage22-project"
        providerKind = "text"
        currentSpendCny = 3
        estimatedIncrementCny = 2
        attemptNumber = 1
    }
    if ($withinBudget.budgetAllowed -ne $true -or $withinBudget.providerCallAllowed -ne $false -or $withinBudget.decision -ne "budget_passed_provider_blocked") {
        throw "Spend guard did not pass budget while keeping provider calls blocked."
    }

    $overBudget = Invoke-Api -Method Post -Path "/api/settings/providers/spend-guard/check" -Body @{
        projectId = "stage22-project"
        providerKind = "text"
        currentSpendCny = 12
        estimatedIncrementCny = 1
        attemptNumber = 1
    }
    if ($overBudget.budgetAllowed -ne $false -or $overBudget.decision -ne "budget_blocked") {
        throw "Spend guard did not block over-budget provider work."
    }

    $retryBlocked = Invoke-Api -Method Post -Path "/api/settings/providers/spend-guard/check" -Body @{
        projectId = "stage22-project"
        providerKind = "text"
        currentSpendCny = 1
        estimatedIncrementCny = 1
        attemptNumber = 3
    }
    if ($retryBlocked.budgetAllowed -ne $false -or $retryBlocked.decision -ne "retry_blocked") {
        throw "Spend guard did not block retry overflow."
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
        throw "Clear API key did not remove Stage 22 secret metadata."
    }

    Write-Host "Stage 22 provider safety passed. Settings: $settingsPath Secrets: $secretStorePath"
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }

    Stop-IntegrationBuildProcesses
}
