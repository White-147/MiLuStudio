param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5399",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Net.Http

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage23b-chunked-upload-build"
$solution = Join-Path $ProjectRoot "backend\control-plane\MiLuStudio.ControlPlane.sln"
$apiDll = Join-Path $buildOutput "MiLuStudio.Api.dll"
$testRoot = Join-Path $ProjectRoot (".tmp\stage23b-chunked-upload\" + ([guid]::NewGuid().ToString("N")))
$storageRoot = Join-Path $testRoot "storage"
$uploadsRoot = Join-Path $testRoot "uploads"
$sqlitePath = Join-Path $testRoot "milu-stage23b-chunked-upload.sqlite3"
$fixturesRoot = Join-Path $testRoot "fixtures"
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
            $_.CommandLine -like "*$buildOutput*"
        } |
        ForEach-Object {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        }

    Start-Sleep -Milliseconds 500
}

function New-ChunkedTextFixture {
    New-Item -ItemType Directory -Force -Path $fixturesRoot | Out-Null
    $path = Join-Path $fixturesRoot "stage23b-chunked-story.txt"
    $targetBytes = (2 * 1024 * 1024) + 12345
    $segment = [System.Text.Encoding]::UTF8.GetBytes("Stage23B chunked upload marker. This payload is intentionally larger than one chunk and remains local to Control API. ")
    $stream = [System.IO.File]::Create($path)
    try {
        $written = 0
        while ($written -lt $targetBytes) {
            $remaining = $targetBytes - $written
            $count = [Math]::Min($segment.Length, $remaining)
            $stream.Write($segment, 0, $count)
            $written += $count
        }
    }
    finally {
        $stream.Dispose()
    }

    return $path
}

function Get-FileChunkBytes {
    param(
        [string]$Path,
        [long]$Offset,
        [int]$Count
    )

    $buffer = New-Object byte[] $Count
    $stream = [System.IO.File]::OpenRead($Path)
    try {
        $stream.Seek($Offset, [System.IO.SeekOrigin]::Begin) | Out-Null
        $read = $stream.Read($buffer, 0, $Count)
        if ($read -ne $Count) {
            throw "Expected to read $Count bytes at offset $Offset, read $read."
        }

        return ,$buffer
    }
    finally {
        $stream.Dispose()
    }
}

function Get-Sha256Hex {
    param([byte[]]$Bytes)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString($sha.ComputeHash($Bytes))).Replace("-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Send-Chunk {
    param(
        [string]$ProjectId,
        [string]$SessionId,
        [int]$ChunkIndex,
        [byte[]]$Bytes
    )

    $client = [System.Net.Http.HttpClient]::new()
    $content = $null
    $request = $null
    try {
        $client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $script:AccessToken)
        $content = [System.Net.Http.ByteArrayContent]::new($Bytes)
        $content.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/octet-stream")
        $request = [System.Net.Http.HttpRequestMessage]::new(
            [System.Net.Http.HttpMethod]::Put,
            "$ApiBaseUrl/api/projects/$ProjectId/assets/upload-sessions/$SessionId/chunks/$ChunkIndex")
        $request.Headers.Add("X-MiLuStudio-Chunk-Sha256", (Get-Sha256Hex -Bytes $Bytes))
        $request.Content = $content

        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) {
            throw "Chunk upload failed: $($response.StatusCode) $body"
        }

        return $body | ConvertFrom-Json
    }
    finally {
        if ($null -ne $request) {
            $request.Dispose()
        }
        elseif ($null -ne $content) {
            $content.Dispose()
        }
        $client.Dispose()
    }
}

function Assert-AssetAnalysis {
    param(
        [string]$ProjectId,
        [string]$AssetId,
        [int]$MinimumChunks
    )

    $analysis = Invoke-Api -Method Get -Path "/api/projects/$ProjectId/assets/$AssetId/analysis"
    Assert-True ($analysis.id -eq $AssetId) "Asset analysis endpoint returned the wrong asset id."
    Assert-True ($analysis.upload.mode -eq "control_api_resumable_chunks") "Asset analysis did not preserve chunked upload mode."
    Assert-True ($analysis.chunkManifestSummary.status -eq "ok") "Asset analysis did not expose a usable chunk manifest."
    Assert-True ([int]$analysis.chunkManifestSummary.totalChunks -ge $MinimumChunks) "Asset analysis chunk manifest did not expose enough chunks."
    Assert-True ($analysis.boundary.uiElectronFileAccess -eq $false) "Asset analysis crossed the UI/Electron file boundary."
    Assert-True ($analysis.boundary.generationPayloadSent -eq $false) "Asset analysis crossed generation payload boundary."
    Assert-True ($analysis.boundary.modelProviderUsed -eq $false) "Asset analysis crossed model provider boundary."

    $serialized = $analysis | ConvertTo-Json -Depth 80
    Assert-True (-not $serialized.Contains($uploadsRoot)) "Asset analysis leaked local uploads path."
    Assert-True (-not $serialized.Contains($storageRoot)) "Asset analysis leaked local storage path."

    return $analysis
}

try {
    Stop-IntegrationBuildProcesses

    if (-not $SkipBuild) {
        & $DotnetPath build $solution --no-restore "-p:OutputPath=$buildOutput\"
        if ($LASTEXITCODE -ne 0) {
            throw ".NET build failed."
        }
    }

    $fixturePath = New-ChunkedTextFixture
    $fileInfo = Get-Item -LiteralPath $fixturePath
    $api = Start-ControlApi

    $authSuffix = ([guid]::NewGuid().ToString("N")).Substring(0, 12)
    $authSession = Invoke-Api -Method Post -Path "/api/auth/register" -Body @{
        email = "stage23b_chunked_$authSuffix@example.local"
        displayName = "Stage 23B Chunked Upload"
        password = "Stage23B-Chunked-Test-Password!"
        deviceFingerprint = "stage23b-chunked-device-$authSuffix"
        deviceName = "Stage 23B Chunked PowerShell Device"
    }
    $script:AccessToken = $authSession.accessToken
    $script:AuthHeaders = @{ Authorization = "Bearer $script:AccessToken" }

    $story = ("Stage23B chunked upload project story keeps validation happy while resumable upload stays under Control API. " * 8)
    $project = Invoke-Api -Method Post -Path "/api/projects" -Body @{
        title = "Stage 23B chunked upload"
        storyText = $story
        mode = "director"
        targetDuration = 45
        aspectRatio = "9:16"
        stylePreset = "stage23b chunked deterministic"
    }

    $chunkSize = 1024 * 1024
    $session = Invoke-Api -Method Post -Path "/api/projects/$($project.id)/assets/upload-sessions" -Body @{
        intent = "storyText"
        originalFileName = [System.IO.Path]::GetFileName($fixturePath)
        contentType = "text/plain"
        fileSize = $fileInfo.Length
        chunkSize = $chunkSize
    }

    Assert-True ($session.status -eq "active") "Chunked upload session did not start active."
    Assert-True ($session.totalChunks -eq 3) "Chunked upload session should have 3 chunks."
    Assert-True ($session.uploadedChunks.Count -eq 0) "Chunked upload session should start with no uploaded chunks."

    $chunk1 = Get-FileChunkBytes -Path $fixturePath -Offset $chunkSize -Count $chunkSize
    $chunk1Result = Send-Chunk -ProjectId $project.id -SessionId $session.id -ChunkIndex 1 -Bytes $chunk1
    Assert-True ($chunk1Result.readyToComplete -eq $false) "Session should not complete after one out-of-order chunk."

    $status = Invoke-Api -Method Get -Path "/api/projects/$($project.id)/assets/upload-sessions/$($session.id)"
    Assert-True (@($status.uploadedChunks).Count -eq 1 -and @($status.uploadedChunks)[0] -eq 1) "Session resume status did not report uploaded chunk 1."

    $chunk0 = Get-FileChunkBytes -Path $fixturePath -Offset 0 -Count $chunkSize
    $chunk0Result = Send-Chunk -ProjectId $project.id -SessionId $session.id -ChunkIndex 0 -Bytes $chunk0
    Assert-True ($chunk0Result.readyToComplete -eq $false) "Session should not complete after two chunks."

    $lastSize = [int]($fileInfo.Length - (2 * $chunkSize))
    $chunk2 = Get-FileChunkBytes -Path $fixturePath -Offset (2 * $chunkSize) -Count $lastSize
    $chunk2Result = Send-Chunk -ProjectId $project.id -SessionId $session.id -ChunkIndex 2 -Bytes $chunk2
    Assert-True ($chunk2Result.readyToComplete -eq $true) "Session should be ready to complete after all chunks."

    $completed = Invoke-Api -Method Post -Path "/api/projects/$($project.id)/assets/upload-sessions/$($session.id)/complete"
    Assert-True ($completed.session.status -eq "completed") "Chunked upload session was not marked completed."
    Assert-True ($completed.session.assetId -eq $completed.asset.id) "Completed session did not reference the created asset."
    Assert-True ($completed.asset.kind -eq "story_text") "Chunked upload asset was not classified as story_text."
    Assert-True ($completed.asset.fileSize -eq $fileInfo.Length) "Chunked upload asset size did not match source file."
    Assert-True ($completed.asset.extractedText -like "*Stage23B chunked upload marker*") "Chunked upload extracted text marker was missing."

    $metadata = $completed.asset.metadataJson | ConvertFrom-Json
    Assert-True ($metadata.upload.mode -eq "control_api_resumable_chunks") "Completed asset did not record chunked upload mode."
    Assert-True ($metadata.upload.chunkingPolicy.status -eq "endpoint_available") "Chunking policy did not record endpoint availability."
    Assert-True ($metadata.parse.generationPayloadSent -eq $false) "Chunked upload crossed generation payload boundary."
    Assert-True ($metadata.parse.modelProviderUsed -eq $false) "Chunked upload crossed model provider boundary."
    Assert-True ($metadata.technical.chunkManifest.status -eq "ok") "Chunked upload did not produce a text chunk manifest."
    $analysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $completed.asset.id -MinimumChunks 2
    Assert-True (@($analysis.chunkManifest.chunks).Count -ge 2) "Chunked upload analysis endpoint did not expose manifest chunks."

    $completedStatus = Invoke-Api -Method Get -Path "/api/projects/$($project.id)/assets/upload-sessions/$($session.id)"
    Assert-True ($completedStatus.status -eq "completed") "Completed session status endpoint did not persist completed state."

    Write-Host "Stage 23B chunked upload passed. Uploads: $uploadsRoot"
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }

    Stop-IntegrationBuildProcesses
}
