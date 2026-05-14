param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5368",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [int]$TimeoutSeconds = 210,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage17-integration-build"
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
    $process = Start-Process -FilePath $DotnetPath -ArgumentList @($apiDll) -WorkingDirectory $buildOutput -WindowStyle Hidden -PassThru
    $startedProcesses.Add($process)
    Wait-ApiHealthy
    if ($process.HasExited) {
        throw "Control API process exited early. Another process may already be bound to $ApiBaseUrl."
    }
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
        Where-Object {
            $_.CommandLine -like "*$buildOutput*" -or
            ($_.CommandLine -like "*$ProjectRoot*" -and ($_.CommandLine -like "*MiLuStudio.Api.dll*" -or $_.CommandLine -like "*MiLuStudio.Worker.dll*"))
        } |
        ForEach-Object {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        }

    Start-Sleep -Milliseconds 500
}

function New-Stage17Story {
    $segment = "Stage17 storyboard edit story: Lin Xi follows a glowing paper crane through a rain-soaked photo studio, finds a missing brother's film roll, discovers the faceless shadow is a future version of herself, and must lock the camera before dawn to pull him back into the real world. "
    return ($segment * 4)
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
                notes = "stage17 integration approve"
            } | Out-Null
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    throw "Timed out while approving checkpoints for $JobId."
}

function Get-DialogueText {
    param([object]$Dialogue)

    if ($null -eq $Dialogue) {
        return ""
    }

    if ($Dialogue -is [array]) {
        return (($Dialogue | ForEach-Object {
            if ($_.speaker -and $_.line) {
                "$($_.speaker): $($_.line)"
            }
            elseif ($_.line) {
                "$($_.line)"
            }
        }) -join "`n")
    }

    return "$Dialogue"
}

function Convert-ToShotEdits {
    param(
        [object[]]$Shots,
        [string]$Marker
    )

    return @($Shots | ForEach-Object -Begin { $index = 0 } -Process {
        $index += 1
        $scene = "$($_.scene)"
        if ($index -eq 1) {
            $scene = "$scene $Marker"
        }

        @{
            shotId = "$($_.shot_id)"
            durationSeconds = [int]$_.duration_seconds
            scene = $scene
            visualAction = "$($_.visual_action)"
            shotSize = "$($_.shot_size)"
            cameraMovement = "$($_.camera.motion)"
            soundNote = "$($_.sound_note)"
            dialogue = Get-DialogueText -Dialogue $_.dialogue
            narration = "$($_.narration)"
        }
    })
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

    $authSuffix = ([guid]::NewGuid().ToString("N")).Substring(0, 12)
    $authSession = Invoke-Api -Method Post -Path "/api/auth/register" -Body @{
        email = "stage17_$authSuffix@example.local"
        displayName = "Stage 17 Integration"
        password = "Stage17-Test-Password!"
        deviceFingerprint = "stage17-device-$authSuffix"
        deviceName = "Stage 17 PowerShell Device"
    }
    if (-not $authSession.license.isActive) {
        throw "Stage 17 integration auth bootstrap did not enable current MVP account access."
    }
    $script:AuthHeaders = @{ Authorization = "Bearer $($authSession.accessToken)" }

    $project = Invoke-Api -Method Post -Path "/api/projects" -Body @{
        title = "Stage 17 storyboard editing"
        storyText = New-Stage17Story
        mode = "director"
        targetDuration = 45
        aspectRatio = "9:16"
        stylePreset = "stage17 deterministic cinematic"
    }

    $job = Invoke-Api -Method Post -Path "/api/projects/$($project.id)/production-jobs" -Body @{ requestedBy = "stage17-integration" }
    $worker = Start-Worker
    $completed = Approve-Checkpoints-UntilComplete -JobId $job.id

    $tasks = Invoke-Api -Method Get -Path "/api/production-jobs/$($completed.id)/tasks"
    $storyboardTask = $tasks | Where-Object { $_.skillName -eq "storyboard_director" } | Select-Object -First 1
    if ($null -eq $storyboardTask -or [string]::IsNullOrWhiteSpace($storyboardTask.outputJson)) {
        throw "Completed job did not produce a storyboard_director output."
    }

    $storyboardEnvelope = $storyboardTask.outputJson | ConvertFrom-Json
    if ($storyboardEnvelope.data.validation_report.profile -ne "cinematic_md_v1") {
        throw "Storyboard output did not preserve cinematic_md_v1 structure."
    }

    $marker = "STAGE17_EDIT_MARKER"
    $editNotes = "stage17 integration save edits"
    $editResponse = Invoke-Api -Method Patch -Path "/api/generation-tasks/$($storyboardTask.id)/storyboard" -Body @{
        notes = $editNotes
        shots = Convert-ToShotEdits -Shots $storyboardEnvelope.data.shots -Marker $marker
    }
    if ($editResponse.status -ne "review") {
        throw "Storyboard edit did not return review status."
    }
    if ($editResponse.resetDownstreamTaskCount -le 0) {
        throw "Storyboard edit did not reset completed downstream tasks."
    }

    $editedTasks = Invoke-Api -Method Get -Path "/api/production-jobs/$($completed.id)/tasks"
    $editedStoryboardTask = $editedTasks | Where-Object { $_.skillName -eq "storyboard_director" } | Select-Object -First 1
    $editedEnvelope = $editedStoryboardTask.outputJson | ConvertFrom-Json
    if ($editedStoryboardTask.status -ne "review" -or $editedStoryboardTask.checkpointNotes -ne $editNotes) {
        throw "Storyboard edit did not return the task to review with notes."
    }
    if ($editedEnvelope.data.shots[0].scene -notlike "*$marker*") {
        throw "Storyboard edit marker was not persisted into the output JSON."
    }
    if ($editedEnvelope.data.stage17_edit_summary.model_provider -ne "none" -or $editedEnvelope.data.stage17_edit_summary.media_generated -ne $false) {
        throw "Storyboard edit summary crossed the no-provider/no-media boundary."
    }

    $downstream = @($editedTasks | Where-Object { $_.queueIndex -gt $editedStoryboardTask.queueIndex })
    if (($downstream | Where-Object { $_.status -ne "waiting" -or $null -ne $_.outputJson }).Count -gt 0) {
        throw "Storyboard edit did not reset all downstream tasks to waiting without output JSON."
    }

    $firstShotId = "$($editedEnvelope.data.shots[0].shot_id)"
    $recomputeNotes = "stage17 single shot recompute notes"
    Invoke-Api -Method Post -Path "/api/generation-tasks/$($editedStoryboardTask.id)/storyboard/shots/$firstShotId/regenerate" -Body @{
        notes = $recomputeNotes
    } | Out-Null

    $recomputedTasks = Invoke-Api -Method Get -Path "/api/production-jobs/$($completed.id)/tasks"
    $recomputedStoryboardTask = $recomputedTasks | Where-Object { $_.skillName -eq "storyboard_director" } | Select-Object -First 1
    $recomputedEnvelope = $recomputedStoryboardTask.outputJson | ConvertFrom-Json
    $flags = @($recomputedEnvelope.data.shots[0].review_flags)
    if ($flags -notcontains "stage17_single_shot_regenerated") {
        throw "Single-shot recompute flag was not persisted."
    }
    if ($recomputedEnvelope.data.stage17_edit_summary.operation -ne "regenerate_single_shot") {
        throw "Single-shot recompute summary did not record the operation."
    }
    if ($recomputedEnvelope.data.shots[0].scene -notlike "*$recomputeNotes*") {
        throw "Single-shot recompute did not consume current notes."
    }

    Write-Host "Stage 17 storyboard editing passed. Job: $($completed.id)"
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }

    Stop-IntegrationBuildProcesses
}
