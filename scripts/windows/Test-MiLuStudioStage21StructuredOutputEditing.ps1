param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5368",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [int]$TimeoutSeconds = 240,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage21-integration-build"
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

function New-Stage21Story {
    $segment = "Stage21 structured edit story: Lin Xi follows a glowing paper crane through a rain-soaked photo studio, finds a missing brother's film roll, discovers the faceless shadow is a future version of herself, and must lock the camera before dawn to pull him back into the real world. "
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
                notes = "stage21 integration approve"
            } | Out-Null
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    throw "Timed out while approving checkpoints for $JobId."
}

function Get-TaskBySkill {
    param(
        [object[]]$Tasks,
        [string]$SkillName
    )

    $taskItems = Get-JsonArrayItems -Value $Tasks -FieldName "tasks"
    $task = $taskItems | Where-Object { $_.skillName -eq $SkillName } | Select-Object -First 1
    if ($null -eq $task) {
        throw "Task for skill '$SkillName' was not found."
    }

    return $task
}

function Get-JsonArrayItems {
    param(
        [object[]]$Value,
        [string]$FieldName
    )

    $items = New-Object System.Collections.Generic.List[object]
    foreach ($item in $Value) {
        if ($null -eq $item) {
            continue
        }

        if ($item -is [System.Array]) {
            foreach ($nested in $item) {
                if ($null -ne $nested) {
                    $items.Add($nested)
                }
            }
            continue
        }

        $items.Add($item)
    }

    if ($items.Count -eq 0) {
        throw "$FieldName must be a non-empty JSON array."
    }

    return ,$items.ToArray()
}

function Assert-JsonProperty {
    param(
        [object]$Value,
        [string]$FieldName,
        [string]$PropertyName
    )

    if (-not ($Value.PSObject.Properties.Name -contains $PropertyName)) {
        $actual = $Value | ConvertTo-Json -Depth 20
        throw "$FieldName[0] is missing '$PropertyName'. Actual value: $actual"
    }
}

function Assert-Stage21Boundary {
    param(
        [object]$Envelope,
        [string]$SkillName
    )

    if ($Envelope.data.stage21_edit_summary.skill_name -ne $SkillName) {
        throw "$SkillName did not record stage21 edit summary."
    }
    if ($Envelope.data.stage21_edit_summary.model_provider -ne "none") {
        throw "$SkillName crossed the no-provider boundary."
    }
    if ($Envelope.data.stage21_edit_summary.media_generated -ne $false) {
        throw "$SkillName crossed the no-media-generation boundary."
    }
    if ($Envelope.data.stage21_edit_summary.media_read -ne $false) {
        throw "$SkillName crossed the no-media-read boundary."
    }
    if ($Envelope.data.stage21_edit_summary.ffmpeg_invoked -ne $false) {
        throw "$SkillName crossed the no-FFmpeg boundary."
    }
}

function Invoke-StructuredEdit {
    param(
        [string]$TaskId,
        [string]$Path,
        [object]$Value,
        [string]$Notes
    )

    return Invoke-Api -Method Patch -Path "/api/generation-tasks/$TaskId/structured-output" -Body @{
        notes = $Notes
        edits = @(
            @{
                path = $Path
                value = $Value
            }
        )
    }
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
        email = "stage21_$authSuffix@example.local"
        displayName = "Stage 21 Integration"
        password = "Stage21-Test-Password!"
        deviceFingerprint = "stage21-device-$authSuffix"
        deviceName = "Stage 21 PowerShell Device"
    }
    if (-not $authSession.license.isActive) {
        throw "Stage 21 integration auth bootstrap did not enable current MVP account access."
    }
    $script:AuthHeaders = @{ Authorization = "Bearer $($authSession.accessToken)" }

    $project = Invoke-Api -Method Post -Path "/api/projects" -Body @{
        title = "Stage 21 structured output editing"
        storyText = New-Stage21Story
        mode = "director"
        targetDuration = 45
        aspectRatio = "9:16"
        stylePreset = "stage21 deterministic cinematic"
    }

    $job = Invoke-Api -Method Post -Path "/api/projects/$($project.id)/production-jobs" -Body @{ requestedBy = "stage21-integration" }
    $worker = Start-Worker
    $completed = Approve-Checkpoints-UntilComplete -JobId $job.id
    Stop-StartedProcess -Process $worker

    $tasks = Get-JsonArrayItems -Value (Invoke-Api -Method Get -Path "/api/production-jobs/$($completed.id)/tasks") -FieldName "tasks"

    $videoTask = Get-TaskBySkill -Tasks $tasks -SkillName "video_prompt_builder"
    $videoEnvelope = $videoTask.outputJson | ConvertFrom-Json
    $videoRequests = Get-JsonArrayItems -Value $videoEnvelope.data.video_requests -FieldName "video_requests"
    Assert-JsonProperty -Value $videoRequests[0] -FieldName "video_requests" -PropertyName "prompt"
    $videoRequests[0].prompt = "$($videoRequests[0].prompt) STAGE21_VIDEO_MARKER"
    $videoResponse = Invoke-StructuredEdit -TaskId $videoTask.id -Path "video_requests" -Value $videoRequests -Notes "stage21 video prompt edit"
    if ($videoResponse.status -ne "completed" -or $videoResponse.resetDownstreamTaskCount -le 0) {
        throw "Video prompt edit did not complete and reset downstream tasks."
    }
    $tasks = Get-JsonArrayItems -Value (Invoke-Api -Method Get -Path "/api/production-jobs/$($completed.id)/tasks") -FieldName "tasks"
    $videoTask = Get-TaskBySkill -Tasks $tasks -SkillName "video_prompt_builder"
    Assert-Stage21Boundary -Envelope ($videoTask.outputJson | ConvertFrom-Json) -SkillName "video_prompt_builder"

    $imageTask = Get-TaskBySkill -Tasks $tasks -SkillName "image_prompt_builder"
    $imageEnvelope = $imageTask.outputJson | ConvertFrom-Json
    $imageRequests = Get-JsonArrayItems -Value $imageEnvelope.data.image_requests -FieldName "image_requests"
    Assert-JsonProperty -Value $imageRequests[0] -FieldName "image_requests" -PropertyName "prompt"
    $imageRequests[0].prompt = "$($imageRequests[0].prompt) STAGE21_IMAGE_MARKER"
    $imageResponse = Invoke-StructuredEdit -TaskId $imageTask.id -Path "image_requests" -Value $imageRequests -Notes "stage21 image prompt edit"
    if ($imageResponse.status -ne "completed" -or $imageResponse.resetDownstreamTaskCount -le 0) {
        throw "Image prompt edit did not complete and reset downstream tasks."
    }
    $tasks = Get-JsonArrayItems -Value (Invoke-Api -Method Get -Path "/api/production-jobs/$($completed.id)/tasks") -FieldName "tasks"
    $imageTask = Get-TaskBySkill -Tasks $tasks -SkillName "image_prompt_builder"
    Assert-Stage21Boundary -Envelope ($imageTask.outputJson | ConvertFrom-Json) -SkillName "image_prompt_builder"

    $styleTask = Get-TaskBySkill -Tasks $tasks -SkillName "style_bible"
    $styleEnvelope = $styleTask.outputJson | ConvertFrom-Json
    $styleResponse = Invoke-StructuredEdit -TaskId $styleTask.id -Path "style_name" -Value "$($styleEnvelope.data.style_name) STAGE21_STYLE_MARKER" -Notes "stage21 style edit"
    if ($styleResponse.status -ne "review" -or $styleResponse.resetDownstreamTaskCount -le 0) {
        throw "Style edit did not return review and reset downstream tasks."
    }
    $tasks = Get-JsonArrayItems -Value (Invoke-Api -Method Get -Path "/api/production-jobs/$($completed.id)/tasks") -FieldName "tasks"
    $styleTask = Get-TaskBySkill -Tasks $tasks -SkillName "style_bible"
    $styleEnvelope = $styleTask.outputJson | ConvertFrom-Json
    if ($styleEnvelope.data.style_name -notlike "*STAGE21_STYLE_MARKER*") {
        throw "Style edit marker was not persisted."
    }
    Assert-Stage21Boundary -Envelope $styleEnvelope -SkillName "style_bible"

    $characterTask = Get-TaskBySkill -Tasks $tasks -SkillName "character_bible"
    $characterEnvelope = $characterTask.outputJson | ConvertFrom-Json
    $characters = Get-JsonArrayItems -Value $characterEnvelope.data.characters -FieldName "characters"
    Assert-JsonProperty -Value $characters[0] -FieldName "characters" -PropertyName "identity"
    $characters[0].identity = "$($characters[0].identity) STAGE21_CHARACTER_MARKER"
    $characterResponse = Invoke-StructuredEdit -TaskId $characterTask.id -Path "characters" -Value $characters -Notes "stage21 character edit"
    if ($characterResponse.status -ne "review" -or $characterResponse.resetDownstreamTaskCount -le 0) {
        throw "Character edit did not return review and reset downstream tasks."
    }
    $tasks = Get-JsonArrayItems -Value (Invoke-Api -Method Get -Path "/api/production-jobs/$($completed.id)/tasks") -FieldName "tasks"
    $characterTask = Get-TaskBySkill -Tasks $tasks -SkillName "character_bible"
    $characterEnvelope = $characterTask.outputJson | ConvertFrom-Json
    if (@($characterEnvelope.data.characters)[0].identity -notlike "*STAGE21_CHARACTER_MARKER*") {
        throw "Character edit marker was not persisted."
    }
    Assert-Stage21Boundary -Envelope $characterEnvelope -SkillName "character_bible"

    $downstream = @($tasks | Where-Object { $_.queueIndex -gt $characterTask.queueIndex })
    if (($downstream | Where-Object { $_.status -ne "waiting" -or $null -ne $_.outputJson }).Count -gt 0) {
        throw "Character edit did not reset all downstream tasks to waiting without output JSON."
    }

    Write-Host "Stage 21 structured output editing passed. Job: $($completed.id)"
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }

    Stop-IntegrationBuildProcesses
}
