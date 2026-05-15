param(
    [string]$ProjectRoot = "D:\code\MiLuStudio",
    [string]$ApiBaseUrl = "http://127.0.0.1:5398",
    [string]$DotnetPath = "D:\soft\program\dotnet\dotnet.exe",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Net.Http
Add-Type -AssemblyName System.IO.Compression.FileSystem
try {
    Add-Type -AssemblyName System.Drawing
    $script:CanCreateOcrFixture = $true
}
catch {
    $script:CanCreateOcrFixture = $false
}

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
. (Join-Path $ProjectRoot "scripts\windows\Set-MiLuStudioEnv.ps1") -ProjectRoot $ProjectRoot | Out-Null

$buildOutput = Join-Path $ProjectRoot ".tmp\stage23b-asset-parsing-build"
$solution = Join-Path $ProjectRoot "backend\control-plane\MiLuStudio.ControlPlane.sln"
$apiDll = Join-Path $buildOutput "MiLuStudio.Api.dll"
$testRoot = Join-Path $ProjectRoot (".tmp\stage23b-asset-parsing\" + ([guid]::NewGuid().ToString("N")))
$storageRoot = Join-Path $testRoot "storage"
$uploadsRoot = Join-Path $testRoot "uploads"
$sqlitePath = Join-Path $testRoot "milu-stage23b-asset-parsing.sqlite3"
$fixturesRoot = Join-Path $testRoot "fixtures"
$ffmpegBin = Join-Path $ProjectRoot "runtime\ffmpeg\bin"
$ffmpegExe = Join-Path $ffmpegBin "ffmpeg.exe"
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
    $env:ControlPlane__FfmpegBinPath = $ffmpegBin
    $env:ControlPlane__AssetVideoFrameLimit = "4"

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

function New-DocxFixture {
    param(
        [string]$Path,
        [string]$Text
    )

    $docxRoot = Join-Path $fixturesRoot "docx-root"
    New-Item -ItemType Directory -Force -Path (Join-Path $docxRoot "word") | Out-Null
    Set-Content -LiteralPath (Join-Path $docxRoot "word\document.xml") -Encoding UTF8 -Value @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:body>
    <w:p>
      <w:r><w:t>$Text</w:t></w:r>
    </w:p>
  </w:body>
</w:document>
"@
    [System.IO.Compression.ZipFile]::CreateFromDirectory($docxRoot, $Path)
}

function New-OcrImageFixture {
    param([string]$Path)

    if (-not $script:CanCreateOcrFixture) {
        return $null
    }

    $bitmap = [System.Drawing.Bitmap]::new(1200, 260)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $font = $null
    $brush = $null
    try {
        $graphics.Clear([System.Drawing.Color]::White)
        $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::SingleBitPerPixelGridFit
        $font = [System.Drawing.Font]::new("Arial", 58, [System.Drawing.FontStyle]::Bold)
        $brush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::Black)
        $graphics.DrawString("STAGE23B OCR MARKER", $font, $brush, 48, 82)
        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
        return $Path
    }
    finally {
        if ($null -ne $brush) {
            $brush.Dispose()
        }
        if ($null -ne $font) {
            $font.Dispose()
        }
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function New-Fixtures {
    New-Item -ItemType Directory -Force -Path $fixturesRoot | Out-Null

    $textPath = Join-Path $fixturesRoot "stage23b-story.txt"
    $textSegment = "Stage23B text chunk marker. A director uploads a long production brief so the backend can split it into stable chunks without sending it to any model provider. "
    Set-Content -LiteralPath $textPath -Encoding UTF8 -Value ($textSegment * 80)

    $docxPath = Join-Path $fixturesRoot "stage23b-docx.docx"
    New-DocxFixture -Path $docxPath -Text (($textSegment * 12) + "Stage23B DOCX marker.")

    $pdfPath = Join-Path $fixturesRoot "stage23b-pdf.pdf"
    $pdfText = "Stage23B PDF embedded text marker. This text should be found by the lightweight local PDF probe and converted into chunk metadata without external generation calls."
    Set-Content -LiteralPath $pdfPath -Encoding ASCII -Value @"
%PDF-1.4
1 0 obj
<< /Length 180 >>
stream
BT
/F1 12 Tf
72 720 Td
($pdfText) Tj
ET
endstream
endobj
trailer
<< /Root 1 0 R >>
%%EOF
"@

    $docPath = Join-Path $fixturesRoot "stage23b-legacy.doc"
    Set-Content -LiteralPath $docPath -Encoding ASCII -Value "Stage23B legacy DOC placeholder. The backend should record parser_unavailable metadata."

    $pngPath = Join-Path $fixturesRoot "stage23b-image.png"
    [System.IO.File]::WriteAllBytes(
        $pngPath,
        [Convert]::FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII="))

    $ocrImagePath = Join-Path $fixturesRoot "stage23b-ocr-image.png"
    $ocrImage = New-OcrImageFixture -Path $ocrImagePath

    $videoPath = Join-Path $fixturesRoot "stage23b-video.mp4"
    if (Test-Path -LiteralPath $ffmpegExe) {
        & $ffmpegExe -y -hide_banner -loglevel error -f lavfi -i "testsrc=size=320x180:rate=12:duration=2" -pix_fmt yuv420p $videoPath
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create Stage 23B video fixture with FFmpeg."
        }
    }

    return @{
        Text = $textPath
        Docx = $docxPath
        Pdf = $pdfPath
        Doc = $docPath
        Image = $pngPath
        OcrImage = $ocrImage
        Video = if (Test-Path -LiteralPath $videoPath) { $videoPath } else { $null }
    }
}

function Assert-NoProviderBoundary {
    param([object]$Metadata)

    Assert-True ($Metadata.parse.generationPayloadSent -eq $false) "Upload metadata crossed generation payload boundary."
    Assert-True ($Metadata.parse.modelProviderUsed -eq $false) "Upload metadata crossed model provider boundary."
}

function Assert-AssetAnalysis {
    param(
        [string]$ProjectId,
        [string]$AssetId,
        [string]$ExpectedManifestStatus,
        [int]$MinimumChunks = 0
    )

    $analysis = Invoke-Api -Method Get -Path "/api/projects/$ProjectId/assets/$AssetId/analysis"
    Assert-True ($analysis.id -eq $AssetId) "Asset analysis endpoint returned the wrong asset id."
    Assert-True ($analysis.analysisSchemaVersion -eq "stage23b_asset_analysis_v1") "Asset analysis did not expose the Stage 23B schema."
    Assert-True ($analysis.boundary.uiElectronFileAccess -eq $false) "Asset analysis crossed the UI/Electron file boundary."
    Assert-True ($analysis.boundary.generationPayloadSent -eq $false) "Asset analysis crossed generation payload boundary."
    Assert-True ($analysis.boundary.modelProviderUsed -eq $false) "Asset analysis crossed model provider boundary."
    Assert-True ($analysis.chunkManifestSummary.status -eq $ExpectedManifestStatus) "Asset analysis chunk manifest status mismatch."
    Assert-True ([int]$analysis.chunkManifestSummary.totalChunks -ge $MinimumChunks) "Asset analysis chunk manifest did not expose enough chunks."

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

    $fixtures = New-Fixtures
    $api = Start-ControlApi

    $authSuffix = ([guid]::NewGuid().ToString("N")).Substring(0, 12)
    $authSession = Invoke-Api -Method Post -Path "/api/auth/register" -Body @{
        email = "stage23b_$authSuffix@example.local"
        displayName = "Stage 23B Asset Parsing"
        password = "Stage23B-Test-Password!"
        deviceFingerprint = "stage23b-device-$authSuffix"
        deviceName = "Stage 23B PowerShell Device"
    }
    $script:AccessToken = $authSession.accessToken
    $script:AuthHeaders = @{ Authorization = "Bearer $script:AccessToken" }

    $story = ("Stage23B project story input keeps the project creation validator happy while asset parsing remains the focus. " * 8)
    $project = Invoke-Api -Method Post -Path "/api/projects" -Body @{
        title = "Stage 23B asset parsing"
        storyText = $story
        mode = "director"
        targetDuration = 45
        aspectRatio = "9:16"
        stylePreset = "stage23b deterministic"
    }

    $textUpload = Invoke-AssetUpload -ProjectId $project.id -Path $fixtures.Text -ContentType "text/plain" -Intent "storyText"
    $textMeta = $textUpload.metadataJson | ConvertFrom-Json
    Assert-True ($textUpload.kind -eq "story_text") "Text upload was not classified as story_text."
    Assert-True ($textMeta.stage -eq "stage23b_document_media_analysis") "Text upload did not record Stage 23B schema stage."
    Assert-True ($textMeta.technical.chunkManifest.status -eq "ok") "Text upload did not produce a chunk manifest."
    Assert-True ([int]$textMeta.technical.chunkManifest.totalChunks -ge 2) "Text fixture should produce multiple chunks."
    Assert-True ($textMeta.upload.chunkingPolicy.preferredChunkBytes -eq 8388608) "Upload chunking policy was not recorded."
    Assert-NoProviderBoundary -Metadata $textMeta
    $textAnalysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $textUpload.id -ExpectedManifestStatus "ok" -MinimumChunks 2
    Assert-True (@($textAnalysis.contentBlocks).Count -ge 1) "Text analysis did not expose content blocks for downstream consumption."

    $docxUpload = Invoke-AssetUpload -ProjectId $project.id -Path $fixtures.Docx -ContentType "application/vnd.openxmlformats-officedocument.wordprocessingml.document" -Intent "storyText"
    $docxMeta = $docxUpload.metadataJson | ConvertFrom-Json
    Assert-True ($docxMeta.technical.parser.status -eq "ok") "DOCX parser did not report ok."
    Assert-True ($docxMeta.technical.chunkManifest.status -eq "ok") "DOCX upload did not produce a chunk manifest."
    Assert-NoProviderBoundary -Metadata $docxMeta
    $docxAnalysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $docxUpload.id -ExpectedManifestStatus "ok" -MinimumChunks 1
    Assert-True ($docxAnalysis.parser.status -eq "ok") "DOCX analysis endpoint did not expose parser status."

    $pdfUpload = Invoke-AssetUpload -ProjectId $project.id -Path $fixtures.Pdf -ContentType "application/pdf" -Intent "storyText"
    $pdfMeta = $pdfUpload.metadataJson | ConvertFrom-Json
    Assert-True ($pdfMeta.technical.parser.status -eq "ok") "PDF embedded-text probe did not report ok."
    Assert-True ($pdfMeta.technical.chunkManifest.status -eq "ok") "PDF upload did not produce a chunk manifest."
    Assert-True ($pdfUpload.extractedText -like "*Stage23B PDF embedded text marker*") "PDF extracted text marker was not returned."
    Assert-NoProviderBoundary -Metadata $pdfMeta
    $pdfAnalysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $pdfUpload.id -ExpectedManifestStatus "ok" -MinimumChunks 1
    Assert-True (@("not_required", "runtime_available_not_invoked_by_default") -contains $pdfAnalysis.ocr.status) "PDF embedded-text analysis should not require OCR."

    $docUpload = Invoke-AssetUpload -ProjectId $project.id -Path $fixtures.Doc -ContentType "application/msword" -Intent "storyText"
    $docMeta = $docUpload.metadataJson | ConvertFrom-Json
    Assert-True ($docMeta.technical.parser.status -eq "parser_unavailable") "Legacy DOC did not record parser_unavailable."
    Assert-True ($docMeta.technical.chunkManifest.status -eq "unavailable") "Legacy DOC should record unavailable chunk manifest."
    Assert-NoProviderBoundary -Metadata $docMeta
    $docAnalysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $docUpload.id -ExpectedManifestStatus "unavailable"
    Assert-True ($docAnalysis.parser.status -eq "parser_unavailable") "Legacy DOC analysis endpoint did not expose parser_unavailable."

    $imageUpload = Invoke-AssetUpload -ProjectId $project.id -Path $fixtures.Image -ContentType "image/png" -Intent "imageReference"
    $imageMeta = $imageUpload.metadataJson | ConvertFrom-Json
    Assert-True ($imageUpload.kind -eq "image_reference") "Image upload was not classified as image_reference."
    Assert-True ($imageMeta.technical.compressionPolicy.backendAdapterOnly -eq $true) "Image compression policy was not backend-adapter-only."
    Assert-True ($imageMeta.technical.ocr.uiElectronFileAccess -eq $false) "Image OCR metadata crossed UI/Electron file boundary."
    Assert-NoProviderBoundary -Metadata $imageMeta
    $imageAnalysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $imageUpload.id -ExpectedManifestStatus "unavailable"
    Assert-True ($imageAnalysis.derivatives.accessPolicy -eq "backend_adapter_only") "Image analysis did not keep derivatives behind backend adapter policy."

    if ($null -ne $fixtures.OcrImage) {
        $ocrImageUpload = Invoke-AssetUpload -ProjectId $project.id -Path $fixtures.OcrImage -ContentType "image/png" -Intent "imageReference"
        $ocrImageMeta = $ocrImageUpload.metadataJson | ConvertFrom-Json
        Assert-True ($ocrImageMeta.technical.ocr.modelProviderUsed -eq $false) "OCR image crossed model provider boundary."
        Assert-True ($ocrImageMeta.technical.ocr.generationPayloadSent -eq $false) "OCR image crossed generation payload boundary."
        Assert-True ($ocrImageMeta.technical.ocr.uiElectronFileAccess -eq $false) "OCR image crossed UI/Electron file boundary."

        if ($ocrImageMeta.technical.ocr.runtimeAvailable -eq $true) {
            Assert-True ($ocrImageMeta.technical.ocr.status -eq "ok") "OCR runtime was available but image OCR did not succeed: $($ocrImageMeta.technical.ocr.status)"
            Assert-True ($ocrImageMeta.technical.ocr.invoked -eq $true) "OCR runtime was available but not invoked."
            Assert-True ($ocrImageUpload.extractedText -like "*STAGE23B*" -or $ocrImageUpload.extractedText -like "*OCR*") "OCR extracted text marker was missing."
            $ocrImageAnalysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $ocrImageUpload.id -ExpectedManifestStatus "ok" -MinimumChunks 1
            Assert-True ($ocrImageAnalysis.ocr.invoked -eq $true) "OCR analysis did not report invocation."
            Assert-True ($ocrImageAnalysis.ocr.extractedTextLength -gt 0) "OCR analysis did not report extracted text length."
        }
        else {
            Assert-True ($ocrImageMeta.technical.ocr.status -eq "runtime_not_configured") "OCR runtime absence did not produce runtime_not_configured."
            Assert-True ($ocrImageMeta.technical.ocr.invoked -eq $false) "OCR runtime was absent but invocation was recorded."
            $ocrImageAnalysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $ocrImageUpload.id -ExpectedManifestStatus "unavailable"
            Assert-True ($ocrImageAnalysis.ocr.runtimeAvailable -eq $false) "OCR analysis reported runtime available unexpectedly."
        }

        Assert-NoProviderBoundary -Metadata $ocrImageMeta
    }
    else {
        Write-Host "System.Drawing was unavailable; OCR image fixture generation skipped while base image OCR fallback metadata remains covered."
    }

    if ($null -ne $fixtures.Video) {
        $videoUpload = Invoke-AssetUpload -ProjectId $project.id -Path $fixtures.Video -ContentType "video/mp4" -Intent "videoReference"
        $videoMeta = $videoUpload.metadataJson | ConvertFrom-Json
        Assert-True ($videoUpload.kind -eq "video_reference") "Video upload was not classified as video_reference."
        Assert-True ($videoMeta.technical.compressionPolicy.backendAdapterOnly -eq $true) "Video compression policy was not backend-adapter-only."
        Assert-True ($videoMeta.technical.frameExtraction.targetFrameCount -eq 4) "Video frame extraction did not use the configured frame target."
        Assert-True ($videoMeta.technical.frameExtraction.actualFrameCount -ge 1) "Video frame extraction did not create any frames."
        Assert-NoProviderBoundary -Metadata $videoMeta
        $videoAnalysis = Assert-AssetAnalysis -ProjectId $project.id -AssetId $videoUpload.id -ExpectedManifestStatus "unavailable"
        Assert-True ($videoAnalysis.derivatives.accessPolicy -eq "backend_adapter_only") "Video analysis did not keep derivatives behind backend adapter policy."
    }
    else {
        Write-Host "FFmpeg was not found at $ffmpegExe; video fixture generation skipped while fallback metadata remains covered by image/media policy checks."
    }

    $assets = Invoke-Api -Method Get -Path "/api/projects/$($project.id)/assets"
    Assert-True (@($assets).Count -ge 5) "Asset repository did not record uploaded Stage 23B fixtures."

    Write-Host "Stage 23B asset parsing passed. Uploads: $uploadsRoot"
}
finally {
    foreach ($process in $startedProcesses) {
        Stop-StartedProcess -Process $process
    }

    Stop-IntegrationBuildProcesses
}
