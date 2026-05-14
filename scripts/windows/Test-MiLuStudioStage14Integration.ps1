param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5368",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [string]$PsqlPath = "D:\soft\program\PostgreSQL\18\bin\psql.exe",
    [int]$TimeoutSeconds = 180,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage14-integration-build"
$solution = Join-Path $ProjectRoot "backend\control-plane\MiLuStudio.ControlPlane.sln"
$apiDll = Join-Path $buildOutput "MiLuStudio.Api.dll"
$workerDll = Join-Path $buildOutput "MiLuStudio.Worker.dll"
$startedProcesses = New-Object System.Collections.Generic.List[System.Diagnostics.Process]
$script:AuthHeaders = @{}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null
    )

    $uri = "$ApiBaseUrl$Path"
    if ($null -eq $Body) {
        return Invoke-RestMethod -Method $Method -Uri $uri -Headers $script:AuthHeaders
    }

    return Invoke-RestMethod `
        -Method $Method `
        -Uri $uri `
        -Headers $script:AuthHeaders `
        -ContentType "application/json; charset=utf-8" `
        -Body ($Body | ConvertTo-Json -Depth 20)
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

function Start-Worker {
    $env:DOTNET_ENVIRONMENT = "Development"
    $process = Start-Process -FilePath $DotnetPath -ArgumentList @($workerDll) -WorkingDirectory $buildOutput -WindowStyle Hidden -PassThru
    $startedProcesses.Add($process)
    Start-Sleep -Seconds 2
    return $process
}

function Stop-StartedProcess {
    param([System.Diagnostics.Process]$Process)

    if ($null -eq $Process -or $Process.HasExited) {
        return
    }

    $processId = $Process.Id
    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    try {
        $Process.WaitForExit(5000) | Out-Null
    }
    catch {
        $deadline = (Get-Date).AddSeconds(5)
        while ((Get-Process -Id $processId -ErrorAction SilentlyContinue) -and (Get-Date) -lt $deadline) {
            Start-Sleep -Milliseconds 200
        }
    }
    Start-Sleep -Milliseconds 500
}

function Stop-IntegrationBuildProcesses {
    Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { $_.CommandLine -like "*$buildOutput*" } |
        ForEach-Object {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        }

    Start-Sleep -Milliseconds 500
}

function Invoke-PsqlCommand {
    param([string]$Sql)

    $oldPassword = $env:PGPASSWORD
    try {
        $env:PGPASSWORD = "root"
        & $PsqlPath -h 127.0.0.1 -p 5432 -U root -d milu -v ON_ERROR_STOP=1 -c $Sql | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "psql command failed."
        }
    }
    finally {
        $env:PGPASSWORD = $oldPassword
    }
}

function New-Stage14Story {
    $segment = "Stage14 integration story: Lin Xi follows a glowing paper crane through a rainy old alley, enters an abandoned photo studio, discovers a damaged film roll left by her missing brother, and must expose a faceless shadow before dawn. "
    return ($segment * 4)
}

function Wait-JobState {
    param(
        [string]$JobId,
        [string[]]$ExpectedStatuses
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $job = Invoke-Api -Method Get -Path "/api/production-jobs/$JobId"
        if ($ExpectedStatuses -contains $job.status) {
            return $job
        }

        if ($job.status -eq "failed") {
            throw "Job $JobId failed: $($job.errorMessage)"
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for job $JobId to reach $($ExpectedStatuses -join ', ')."
}

function Approve-Checkpoints-UntilComplete {
    param([string]$JobId)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $job = Invoke-Api -Method Get -Path "/api/production-jobs/$JobId"
        if ($job.status -eq "completed") {
            return $job
        }

        if ($job.status -eq "failed") {
            throw "Job $JobId failed: $($job.errorMessage)"
        }

        if ($job.status -eq "paused") {
            Invoke-Api -Method Post -Path "/api/production-jobs/$JobId/checkpoint" -Body @{
                approved = $true
                notes = "stage14 integration approve"
            } | Out-Null
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    throw "Timed out while approving checkpoints for $JobId."
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

    $authSuffix = ([guid]::NewGuid().ToString("N")).Substring(0, 12)
    $authSession = Invoke-Api -Method Post -Path "/api/auth/register" -Body @{
        email = "stage14_$authSuffix@example.local"
        displayName = "Stage 14 Integration"
        password = "Stage14-Test-Password!"
        deviceFingerprint = "stage14-device-$authSuffix"
        deviceName = "Stage 14 PowerShell Device"
    }
    if (-not $authSession.license.isActive) {
        throw "Stage 14 integration auth bootstrap did not enable current MVP account access."
    }
    $script:AuthHeaders = @{ Authorization = "Bearer $($authSession.accessToken)" }

    $story = New-Stage14Story
    $created = Invoke-Api -Method Post -Path "/api/projects" -Body @{
        title = "Stage 14 input path test"
        storyText = $story
        mode = "director"
        targetDuration = 45
        aspectRatio = "9:16"
        stylePreset = "stage14 rainy suspense"
    }

    $updated = Invoke-Api -Method Patch -Path "/api/projects/$($created.id)" -Body @{
        title = "Stage 14 input path saved"
        storyText = "$story STAGE14_UPDATED_MARKER confirms the Worker consumed the latest saved story input."
        mode = "fast"
        targetDuration = 60
        aspectRatio = "16:9"
        stylePreset = "stage14 cinematic blue rain"
    }

    if ($updated.storyText -notlike "*STAGE14_UPDATED_MARKER*") {
        throw "Project story input was not updated through Control API."
    }

    $firstJob = Invoke-Api -Method Post -Path "/api/projects/$($updated.id)/production-jobs" -Body @{ requestedBy = "stage14-integration" }
    $regeneratedJob = Invoke-Api -Method Post -Path "/api/projects/$($updated.id)/production-jobs" -Body @{ requestedBy = "stage14-integration" }
    if ($firstJob.id -eq $regeneratedJob.id) {
        throw "Regenerating did not create a fresh job for the current project input."
    }
    $retiredJob = Invoke-Api -Method Get -Path "/api/production-jobs/$($firstJob.id)"
    if ($retiredJob.status -ne "failed" -or [string]::IsNullOrWhiteSpace($retiredJob.errorMessage)) {
        throw "Previous active job was not retired before regenerating."
    }
    $firstJob = $regeneratedJob

    Invoke-PsqlCommand -Sql "update generation_tasks set status='running', locked_by='stage14-stale-worker', locked_until=now() - interval '1 second' where job_id='$($firstJob.id)' and queue_index=0;"
    $worker = Start-Worker
    $pausedJob = Wait-JobState -JobId $firstJob.id -ExpectedStatuses @("paused", "completed")

    Stop-StartedProcess -Process $api
    $api = Start-ControlApi
    Invoke-Api -Method Get -Path "/api/production-jobs/$($firstJob.id)" | Out-Null

    if ($pausedJob.status -eq "paused") {
        Invoke-Api -Method Post -Path "/api/production-jobs/$($firstJob.id)/checkpoint" -Body @{
            approved = $true
            notes = "checkpoint recovery after API restart"
        } | Out-Null
    }

    Stop-StartedProcess -Process $worker
    $worker = Start-Worker
    $completed = Approve-Checkpoints-UntilComplete -JobId $firstJob.id
    $tasks = Invoke-Api -Method Get -Path "/api/production-jobs/$($firstJob.id)/tasks"
    $storyTask = $tasks | Where-Object { $_.skillName -eq "story_intake" } | Select-Object -First 1
    if ($null -eq $storyTask -or $storyTask.inputJson -notlike "*STAGE14_UPDATED_MARKER*") {
        throw "Worker did not consume the latest story input saved through Control API."
    }

    $retryProject = Invoke-Api -Method Post -Path "/api/projects" -Body @{
        title = "Stage 14 checkpoint reject retry"
        storyText = $story
        mode = "director"
        targetDuration = 45
        aspectRatio = "9:16"
        stylePreset = "stage14 light comic"
    }
    $retryJob = Invoke-Api -Method Post -Path "/api/projects/$($retryProject.id)/production-jobs" -Body @{ requestedBy = "stage14-integration" }
    Wait-JobState -JobId $retryJob.id -ExpectedStatuses @("paused") | Out-Null
    Invoke-Api -Method Post -Path "/api/production-jobs/$($retryJob.id)/checkpoint" -Body @{
        approved = $false
        notes = "stage14 integration reject"
    } | Out-Null
    $failed = Wait-JobState -JobId $retryJob.id -ExpectedStatuses @("failed")
    if ($failed.errorMessage -notlike "*stage14 integration reject*") {
        throw "Checkpoint reject notes were not persisted as the failure reason."
    }
    Invoke-Api -Method Post -Path "/api/production-jobs/$($retryJob.id)/retry" -Body @{} | Out-Null
    Wait-JobState -JobId $retryJob.id -ExpectedStatuses @("running", "paused") | Out-Null

    Write-Host "Stage 14 integration passed. Completed job: $($completed.id)"
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }

    Stop-IntegrationBuildProcesses
}
