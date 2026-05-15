param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5402",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [int]$TimeoutSeconds = 90,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Net.Http

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage23c-asset-consumption-build"
$solution = Join-Path $ProjectRoot "backend\control-plane\MiLuStudio.ControlPlane.sln"
$apiDll = Join-Path $buildOutput "MiLuStudio.Api.dll"
$workerDll = Join-Path $buildOutput "MiLuStudio.Worker.dll"
$testRoot = Join-Path $ProjectRoot (".tmp\stage23c-asset-consumption\" + ([guid]::NewGuid().ToString("N")))
$storageRoot = Join-Path $testRoot "storage"
$uploadsRoot = Join-Path $testRoot "uploads"
$fixturesRoot = Join-Path $testRoot "fixtures"
$sqlitePath = Join-Path $testRoot "milu-stage23c-asset-consumption.sqlite3"
$startedProcesses = New-Object System.Collections.Generic.List[System.Diagnostics.Process]
$script:AuthHeaders = @{}
$script:AccessToken = ""

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

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
            $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
            $detail = $reader.ReadToEnd()
        }

        throw "API $Method $Path failed: $($_.Exception.Message) $detail"
    }
}

function Invoke-AssetUpload {
    param(
        [string]$ProjectId,
        [string]$Path,
        [string]$ContentType,
        [string]$Intent
    )

    $client = [System.Net.Http.HttpClient]::new()
    $form = [System.Net.Http.MultipartFormDataContent]::new()
    $fileStream = $null
    try {
        $client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $script:AccessToken)
        $fileStream = [System.IO.File]::OpenRead($Path)
        $fileContent = [System.Net.Http.StreamContent]::new($fileStream)
        $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse($ContentType)
        $form.Add($fileContent, "file", [System.IO.Path]::GetFileName($Path))
        $form.Add([System.Net.Http.StringContent]::new($Intent), "intent")

        $response = $client.PostAsync("$ApiBaseUrl/api/projects/$ProjectId/assets/upload", $form).GetAwaiter().GetResult()
        $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) {
            throw "Asset upload failed: $($response.StatusCode) $body"
        }

        return $body | ConvertFrom-Json
    }
    finally {
        if ($null -ne $form) {
            $form.Dispose()
        }
        if ($null -ne $fileStream) {
            $fileStream.Dispose()
        }
        $client.Dispose()
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
    $env:ControlPlane__RepositoryProvider = "SQLite"
    $env:ConnectionStrings__MiLuStudioControlPlane = "Data Source=$sqlitePath"
    $env:ControlPlane__MigrationsPath = Join-Path $ProjectRoot "backend\control-plane\db\sqlite"
    $env:ControlPlane__StorageRoot = $storageRoot
    $env:ControlPlane__UploadsRoot = $uploadsRoot

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
    $env:ControlPlane__RepositoryProvider = "SQLite"
    $env:ConnectionStrings__MiLuStudioControlPlane = "Data Source=$sqlitePath"
    $env:ControlPlane__MigrationsPath = Join-Path $ProjectRoot "backend\control-plane\db\sqlite"
    $env:ControlPlane__StorageRoot = $storageRoot
    $env:ControlPlane__UploadsRoot = $uploadsRoot

    $process = Start-Process -FilePath $DotnetPath -ArgumentList @($workerDll) -WorkingDirectory $buildOutput -WindowStyle Hidden -PassThru
    $startedProcesses.Add($process)
    Start-Sleep -Seconds 2
    if ($process.HasExited) {
        throw "Worker process exited early."
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
        Where-Object { $_.CommandLine -like "*$buildOutput*" } |
        ForEach-Object {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        }

    Start-Sleep -Milliseconds 500
}

function New-ProjectStory {
    $segment = "PROJECT_FALLBACK_MARKER A saved project story follows a lantern maker who repairs memory lamps in a rainy old market and tries to protect a missing friend's final message. "
    return ($segment * 5)
}

function New-AssetStory {
    $segment = "ASSET_ANALYSIS_STORY_MARKER A uploaded story asset follows Mira, a precise field engineer, as she discovers a hidden signal inside a damaged festival drone and turns the city square into a rescue map. "
    return ($segment * 6)
}

function New-ReferenceImage {
    param([string]$Path)

    $pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII="
    [System.IO.File]::WriteAllBytes($Path, [System.Convert]::FromBase64String($pngBase64))
}

function Wait-StoryIntakeTask {
    param([string]$JobId)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $job = Invoke-Api -Method Get -Path "/api/production-jobs/$JobId"
        if ($job.status -eq "failed") {
            throw "Job $JobId failed: $($job.errorMessage)"
        }

        $tasks = Invoke-Api -Method Get -Path "/api/production-jobs/$JobId/tasks"
        $storyTask = $tasks | Where-Object { $_.skillName -eq "story_intake" } | Select-Object -First 1
        if ($null -ne $storyTask -and $storyTask.status -eq "completed" -and -not [string]::IsNullOrWhiteSpace($storyTask.inputJson)) {
            return $storyTask
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for story_intake to complete for $JobId."
}

function Wait-TaskInputJson {
    param(
        [string]$JobId,
        [string]$SkillName
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $job = Invoke-Api -Method Get -Path "/api/production-jobs/$JobId"
        if ($job.status -eq "failed") {
            throw "Job $JobId failed: $($job.errorMessage)"
        }

        if ($job.status -eq "paused") {
            Invoke-Api -Method Post -Path "/api/production-jobs/$JobId/checkpoint" -Body @{
                approved = $true
                notes = "stage23c asset consumption approve"
            } | Out-Null
        }

        $tasks = Invoke-Api -Method Get -Path "/api/production-jobs/$JobId/tasks"
        $targetTask = $tasks | Where-Object { $_.skillName -eq $SkillName } | Select-Object -First 1
        $inputText = if ($null -ne $targetTask -and $null -ne $targetTask.inputJson) { [string]$targetTask.inputJson } else { "" }
        if ($null -ne $targetTask -and $targetTask.status -ne "waiting" -and $inputText.Trim().Length -gt 0) {
            return $targetTask
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for $SkillName input JSON for $JobId."
}

try {
    Stop-IntegrationBuildProcesses

    if (-not $SkipBuild) {
        & $DotnetPath build $solution --no-restore "-p:OutputPath=$buildOutput\"
        if ($LASTEXITCODE -ne 0) {
            throw ".NET build failed."
        }
    }

    New-Item -ItemType Directory -Force -Path $fixturesRoot | Out-Null
    $assetStoryPath = Join-Path $fixturesRoot "stage23c-story-asset.txt"
    $referenceImagePath = Join-Path $fixturesRoot "stage23c-image-reference.png"
    Set-Content -LiteralPath $assetStoryPath -Encoding UTF8 -Value (New-AssetStory)
    New-ReferenceImage -Path $referenceImagePath

    $api = Start-ControlApi
    Invoke-Api -Method Post -Path "/api/system/migrations/apply" | Out-Null

    $authSuffix = ([guid]::NewGuid().ToString("N")).Substring(0, 12)
    $authSession = Invoke-Api -Method Post -Path "/api/auth/register" -Body @{
        email = "stage23c_asset_$authSuffix@example.local"
        displayName = "Stage 23C Asset Consumption"
        password = "Stage23C-Test-Password!"
        deviceFingerprint = "stage23c-asset-device-$authSuffix"
        deviceName = "Stage 23C Asset Consumption PowerShell Device"
    }
    $script:AccessToken = $authSession.accessToken
    $script:AuthHeaders = @{ Authorization = "Bearer $script:AccessToken" }

    $created = Invoke-Api -Method Post -Path "/api/projects" -Body @{
        title = "Stage 23C asset consumption"
        storyText = (New-ProjectStory)
        mode = "director"
        targetDuration = 45
        aspectRatio = "9:16"
        stylePreset = "stage23c local asset analysis"
    }

    Assert-True ($created.storyText -like "*PROJECT_FALLBACK_MARKER*") "Project fallback story marker was not saved."
    Assert-True ($created.storyText -notlike "*ASSET_ANALYSIS_STORY_MARKER*") "Project story unexpectedly contains the asset marker."

    $uploaded = Invoke-AssetUpload -ProjectId $created.id -Path $assetStoryPath -ContentType "text/plain" -Intent "storyText"
    Assert-True ($uploaded.extractedText -like "*ASSET_ANALYSIS_STORY_MARKER*") "Uploaded asset did not expose extracted text."

    $uploadedImage = Invoke-AssetUpload -ProjectId $created.id -Path $referenceImagePath -ContentType "image/png" -Intent "imageReference"
    Assert-True ($uploadedImage.kind -eq "image_reference") "Uploaded reference image was not classified as image_reference."

    $analysis = Invoke-Api -Method Get -Path "/api/projects/$($created.id)/assets/$($uploaded.id)/analysis"
    Assert-True ($analysis.chunkManifestSummary.status -eq "ok") "Asset analysis chunk manifest is not ok."
    Assert-True ($analysis.chunkManifestSummary.usableAsStoryCandidate -eq $true) "Asset analysis is not marked as story candidate."

    $imageAnalysis = Invoke-Api -Method Get -Path "/api/projects/$($created.id)/assets/$($uploadedImage.id)/analysis"
    Assert-True ($imageAnalysis.kind -eq "image_reference") "Reference image analysis endpoint returned the wrong kind."
    Assert-True ($imageAnalysis.derivatives.accessPolicy -eq "backend_adapter_only") "Reference image derivatives are not backend-adapter-only."

    $job = Invoke-Api -Method Post -Path "/api/projects/$($created.id)/production-jobs" -Body @{ requestedBy = "stage23c-asset-consumption" }
    $worker = Start-Worker
    $storyTask = Wait-StoryIntakeTask -JobId $job.id
    $input = $storyTask.inputJson | ConvertFrom-Json

    Assert-True ($input.story_text -like "*ASSET_ANALYSIS_STORY_MARKER*") "story_intake did not consume the uploaded asset analysis text."
    Assert-True ($input.story_text -notlike "*PROJECT_FALLBACK_MARKER*") "story_intake fell back to project story instead of asset analysis text."

    $imagePromptTask = Wait-TaskInputJson -JobId $job.id -SkillName "image_prompt_builder"
    $imagePromptInput = $imagePromptTask.inputJson | ConvertFrom-Json
    Assert-True ($imagePromptInput.asset_analysis.image_reference_count -ge 1) "image_prompt_builder did not receive reference image asset analysis."
    $imageReferenceIds = @($imagePromptInput.asset_analysis.image_references | ForEach-Object { $_.asset_id })
    Assert-True ($imageReferenceIds -contains $uploadedImage.id) "image_prompt_builder asset analysis did not include the uploaded image reference id."
    Assert-True ($imagePromptInput.asset_analysis.media_access_policy -eq "backend_adapter_only") "image_prompt_builder asset analysis boundary changed."

    Write-Host "Stage 23C asset analysis consumption passed."
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }
    Stop-IntegrationBuildProcesses
}
