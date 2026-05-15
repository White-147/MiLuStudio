namespace MiLuStudio.Infrastructure.Assets;

using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Infrastructure.Configuration;
using CultureInfo = global::System.Globalization.CultureInfo;
using NumberStyles = global::System.Globalization.NumberStyles;
using JsonDocument = global::System.Text.Json.JsonDocument;
using JsonElement = global::System.Text.Json.JsonElement;
using JsonException = global::System.Text.Json.JsonException;
using JsonValueKind = global::System.Text.Json.JsonValueKind;
using Process = global::System.Diagnostics.Process;
using ProcessStartInfo = global::System.Diagnostics.ProcessStartInfo;
using Regex = global::System.Text.RegularExpressions.Regex;
using RegexOptions = global::System.Text.RegularExpressions.RegexOptions;
using TextEncoding = global::System.Text.Encoding;
using XDocument = global::System.Xml.Linq.XDocument;
using XElement = global::System.Xml.Linq.XElement;
using ZipFile = global::System.IO.Compression.ZipFile;

public sealed class FfmpegAssetTechnicalAnalyzer : IAssetTechnicalAnalyzer
{
    private const int MaxExtractedTextCharacters = 120_000;
    private const int TextChunkSizeCharacters = 2_800;
    private const int TextChunkOverlapCharacters = 160;
    private const int MaxChunkPreviewCharacters = 220;
    private const int MaxOcrTextCharacters = 40_000;
    private const int MaxOcrAttemptErrorCharacters = 1_200;
    private const int MaxDocxStructurePreviewBlocks = 40;
    private const int MaxDocxBlockPreviewCharacters = 180;
    private const int MaxPdfDecodedStreamBytes = 4_000_000;
    private const int MaxPdfStreamDecodeAttempts = 64;
    private const int ImagePreviewMaxWidth = 1_280;
    private const int VideoReviewProxySeconds = 6;
    private const int DefaultPdfRasterizerDpi = 180;
    private const int DefaultPdfRasterizerPageLimit = 3;

    private static readonly Regex PdfLiteralTextRegex = new(
        @"\((?<text>(?:\\.|[^\\)])*)\)\s*Tj",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex PdfHexTextRegex = new(
        @"<(?<hex>(?:[0-9A-Fa-f]\s*){2,})>\s*Tj",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex PdfArrayTextRegex = new(
        @"\[(?<array>.*?)\]\s*TJ",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex PdfArrayLiteralRegex = new(
        @"\((?<text>(?:\\.|[^\\)])*)\)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex PdfHexValueRegex = new(
        @"<(?<hex>(?:[0-9A-Fa-f]\s*){2,})>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex PdfStreamRegex = new(
        @"(?<dictionary><<.*?>>)\s*stream(?<body>.*?)endstream",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex PdfStreamLengthRegex = new(
        @"/Length\s+(?<length>\d+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PdfFlateDecodeRegex = new(
        @"/Filter\s*(?:\[[^\]]*)?/FlateDecode",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly HashSet<string> PlainTextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "txt", "md", "markdown", "csv", "json", "srt", "ass", "vtt", "log", "xml", "yaml", "yml", "rtf"
    };

    private static readonly HashSet<string> OcrCandidateExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "png", "jpg", "jpeg", "webp", "bmp", "tif", "tiff", "heic", "heif", "pdf"
    };

    private static readonly HashSet<string> DirectOcrImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "png", "jpg", "jpeg", "webp", "bmp", "tif", "tiff"
    };

    private static readonly string[] DefaultOcrCandidatePaths =
    [
        "D:\\code\\MiLuStudio\\runtime\\tesseract\\tesseract.exe",
        "D:\\tools\\tesseract\\tesseract.exe"
    ];

    private static readonly string[] DefaultPdfRasterizerCandidatePaths =
    [
        "D:\\code\\MiLuStudio\\runtime\\poppler\\Library\\bin\\pdftoppm.exe",
        "D:\\code\\MiLuStudio\\runtime\\poppler\\bin\\pdftoppm.exe",
        "D:\\tools\\poppler\\Library\\bin\\pdftoppm.exe",
        "D:\\tools\\poppler\\bin\\pdftoppm.exe"
    ];

    private static readonly string[] DefaultOcrLanguages = ["chi_sim+eng", "eng"];

    private readonly ControlPlaneOptions _options;

    public FfmpegAssetTechnicalAnalyzer(IOptions<ControlPlaneOptions> options)
    {
        _options = options.Value;
    }

    public async Task<ProjectAssetTechnicalAnalysis> AnalyzeAsync(
        StoredProjectAssetFile file,
        string kind,
        CancellationToken cancellationToken)
    {
        if (kind == "story_text")
        {
            return await AnalyzeTextAsync(file, cancellationToken);
        }

        if (kind is "image_reference" or "video_reference")
        {
            return await AnalyzeMediaAsync(file, kind, cancellationToken);
        }

        return new ProjectAssetTechnicalAnalysis(
            "metadata_only",
            "File saved and registered. Stage 23B currently records base metadata for this asset kind.",
            null,
            [],
            new Dictionary<string, object?>
            {
                ["engine"] = "stage23b_metadata_only",
                ["extension"] = file.Extension,
                ["generationPayloadSent"] = false,
                ["modelProviderUsed"] = false
            });
    }

    private async Task<ProjectAssetTechnicalAnalysis> AnalyzeTextAsync(
        StoredProjectAssetFile file,
        CancellationToken cancellationToken)
    {
        var metadata = CreateDocumentMetadata(file, "stage23b_document_parser");

        if (PlainTextExtensions.Contains(file.Extension))
        {
            var text = await ReadTextFileAsync(file.LocalPath, cancellationToken);
            var extractedText = Truncate(text, MaxExtractedTextCharacters);
            AddTextManifest(metadata, extractedText, "plain_text", "document_body", text.Length > extractedText.Length);
            metadata["parser"] = new Dictionary<string, object?>
            {
                ["engine"] = "stage23b_plain_text_reader",
                ["status"] = "ok",
                ["encoding"] = "utf8_with_bom_detection"
            };

            return new ProjectAssetTechnicalAnalysis(
                "ok",
                "Text parsed and Stage 23B chunk manifest generated.",
                extractedText,
                [],
                metadata);
        }

        if (string.Equals(file.Extension, "docx", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var docx = ExtractDocxText(file.LocalPath);
                var extractedText = string.IsNullOrWhiteSpace(docx.Text) ? null : Truncate(docx.Text, MaxExtractedTextCharacters);
                metadata["parser"] = new Dictionary<string, object?>
                {
                    ["engine"] = "stage23c_docx_zip_xml_structured_reader",
                    ["status"] = extractedText is null ? "empty_text" : "ok"
                };
                metadata["documentStructure"] = docx.StructureMetadata;

                if (extractedText is null)
                {
                    AddUnavailableChunkManifest(metadata, "docx_empty_text");
                }
                else
                {
                    AddTextManifest(metadata, extractedText, "docx_structured", "document_body", docx.Text.Length > extractedText.Length);
                }

                return new ProjectAssetTechnicalAnalysis(
                    extractedText is null ? "metadata_only" : "ok",
                    extractedText is null ? "DOCX saved, but no structured text was extracted." : "DOCX structure parsed and Stage 23C chunk manifest generated.",
                    extractedText,
                    [],
                    metadata);
            }
            catch (Exception error) when (error is IOException or InvalidDataException or global::System.Xml.XmlException)
            {
                metadata["parser"] = new Dictionary<string, object?>
                {
                    ["engine"] = "stage23c_docx_zip_xml_structured_reader",
                    ["status"] = "parser_failed",
                    ["message"] = Truncate(error.Message, 500)
                };
                AddUnavailableChunkManifest(metadata, "docx_parser_failed");

                return new ProjectAssetTechnicalAnalysis(
                    "metadata_only",
                    "DOCX saved, but the local ZIP/XML parser could not extract body text.",
                    null,
                    [],
                    metadata);
            }
        }

        if (string.Equals(file.Extension, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdf = ExtractPdfText(file.LocalPath);
            var extractedText = string.IsNullOrWhiteSpace(pdf.Text) ? null : Truncate(pdf.Text, MaxExtractedTextCharacters);
            metadata["parser"] = new Dictionary<string, object?>
            {
                ["engine"] = "stage23c_pdf_embedded_text_probe",
                ["status"] = extractedText is null ? "ocr_required" : "ok",
                ["textObjectCount"] = pdf.TextObjectCount,
                ["streamCount"] = pdf.StreamCount,
                ["decodedStreamCount"] = pdf.DecodedStreamCount,
                ["failedStreamCount"] = pdf.FailedStreamCount,
                ["warnings"] = pdf.Warnings
            };

            if (extractedText is null)
            {
                var rasterOcr = await AnalyzeScannedPdfOcrAsync(file, cancellationToken);
                metadata["pdfRasterizer"] = rasterOcr.RasterizerMetadata;
                metadata["ocr"] = rasterOcr.OcrMetadata;

                if (rasterOcr.ExtractedText is null)
                {
                    AddUnavailableChunkManifest(metadata, rasterOcr.UnavailableChunkReason);
                }
                else
                {
                    extractedText = rasterOcr.ExtractedText;
                    AddTextManifest(metadata, rasterOcr.ExtractedText, "pdf_raster_ocr", "document_body", rasterOcr.Truncated);
                }
            }
            else
            {
                metadata["ocr"] = BuildOcrPlan(file, "not_required");
                AddTextManifest(metadata, extractedText, "pdf", "document_body", pdf.Text.Length > extractedText.Length);
            }

            return new ProjectAssetTechnicalAnalysis(
                extractedText is null ? "metadata_only" : "ok",
                extractedText is null
                    ? "PDF saved. No embedded text was found; backend PDF rasterizer/OCR fallback metadata was recorded."
                    : metadata.ContainsKey("pdfRasterizer")
                        ? "PDF scanned pages rasterized by the backend adapter and OCR chunk manifest generated."
                        : "PDF embedded text parsed and Stage 23B chunk manifest generated.",
                extractedText,
                [],
                metadata);
        }

        if (string.Equals(file.Extension, "doc", StringComparison.OrdinalIgnoreCase))
        {
            metadata["parser"] = new Dictionary<string, object?>
            {
                ["engine"] = "stage23b_legacy_doc_boundary",
                ["status"] = "parser_unavailable",
                ["reason"] = "Binary .doc requires an explicit backend converter runtime; UI and Electron must not invoke Office automation."
            };
            metadata["ocr"] = BuildOcrPlan(file, "not_applicable");
            metadata["degradation"] = new Dictionary<string, object?>
            {
                ["kind"] = "legacy_doc_parser_unavailable",
                ["uploadAccepted"] = true,
                ["requiresBackendRuntime"] = true,
                ["suggestedRuntimeRoot"] = "D:\\code\\MiLuStudio\\runtime"
            };
            AddUnavailableChunkManifest(metadata, "legacy_doc_parser_unavailable");

            return new ProjectAssetTechnicalAnalysis(
                "metadata_only",
                "Legacy DOC saved. Stage 23B records parser_unavailable metadata until a backend converter runtime is installed.",
                null,
                [],
                metadata);
        }

        metadata["parser"] = new Dictionary<string, object?>
        {
            ["engine"] = "stage23b_document_parser",
            ["status"] = "unsupported_extension"
        };
        AddUnavailableChunkManifest(metadata, "unsupported_text_extension");

        return new ProjectAssetTechnicalAnalysis(
            "metadata_only",
            "Text-like attachment saved; this extension currently records structured metadata only.",
            null,
            [],
            metadata);
    }

    private async Task<ProjectAssetTechnicalAnalysis> AnalyzeMediaAsync(
        StoredProjectAssetFile file,
        string kind,
        CancellationToken cancellationToken)
    {
        var metadata = new Dictionary<string, object?>
        {
            ["engine"] = "stage23b_ffmpeg_media_analyzer",
            ["extension"] = file.Extension,
            ["ffmpegAllowed"] = true,
            ["generationPayloadSent"] = false,
            ["modelProviderUsed"] = false,
            ["compressionPolicy"] = BuildCompressionPolicy(kind)
        };

        string? ocrExtractedText = null;
        if (kind == "image_reference")
        {
            var ocr = await AnalyzeOcrAsync(file, cancellationToken);
            metadata["ocr"] = ocr.Metadata;
            ocrExtractedText = ocr.ExtractedText;
            if (ocr.ExtractedText is null)
            {
                AddUnavailableChunkManifest(metadata, ocr.UnavailableChunkReason);
            }
            else
            {
                AddTextManifest(metadata, ocr.ExtractedText, "image_ocr", "image_ocr_text", ocr.Truncated);
            }
        }

        var derivatives = new List<string>();
        var derivativeDetails = new List<Dictionary<string, object?>>();
        var ffprobePath = ResolveToolPath("ffprobe.exe");
        var ffmpegPath = ResolveToolPath("ffmpeg.exe");

        if (!File.Exists(ffprobePath) || !File.Exists(ffmpegPath))
        {
            metadata["ffmpegAvailable"] = false;
            metadata["expectedToolDirectory"] = _options.FfmpegBinPath;
            metadata["derivativeDetails"] = derivativeDetails;

            return new ProjectAssetTechnicalAnalysis(
                string.IsNullOrWhiteSpace(ocrExtractedText) ? "metadata_only" : "ok",
                string.IsNullOrWhiteSpace(ocrExtractedText)
                    ? "Media saved. Project FFmpeg runtime was not found, so Stage 23B recorded structured fallback metadata."
                    : "Media saved. OCR text was extracted; project FFmpeg runtime was not found, so media derivatives were skipped.",
                ocrExtractedText,
                derivatives,
                metadata);
        }

        metadata["ffmpegAvailable"] = true;
        metadata["ffmpegBinPath"] = _options.FfmpegBinPath;

        var probe = await RunToolAsync(
            ffprobePath,
            [
                "-v", "error",
                "-show_entries", "format=format_name,duration,size,bit_rate:stream=index,codec_type,codec_name,width,height,avg_frame_rate,duration",
                "-of", "json",
                file.LocalPath
            ],
            TimeSpan.FromSeconds(Math.Max(5, _options.AssetParseTimeoutSeconds)),
            cancellationToken);

        metadata["ffprobeExitCode"] = probe.ExitCode;
        metadata["ffprobeJson"] = Truncate(probe.Stdout, 32_000);
        metadata["ffprobeError"] = Truncate(probe.Stderr, 4_000);
        metadata["probeSummary"] = BuildProbeSummary(probe.Stdout);

        var analysisDirectory = Path.Combine(Path.GetDirectoryName(file.LocalPath) ?? ".", "analysis");
        Directory.CreateDirectory(analysisDirectory);

        await CreateThumbnailAsync(
            file,
            ffmpegPath,
            analysisDirectory,
            derivatives,
            derivativeDetails,
            metadata,
            cancellationToken);

        if (kind == "image_reference")
        {
            await CreateImagePreviewAsync(
                file,
                ffmpegPath,
                analysisDirectory,
                derivatives,
                derivativeDetails,
                metadata,
                cancellationToken);
        }

        if (kind == "video_reference")
        {
            await ExtractVideoFramesAsync(
                file,
                ffmpegPath,
                probe.Stdout,
                analysisDirectory,
                derivatives,
                derivativeDetails,
                metadata,
                cancellationToken);

            await CreateVideoReviewProxyAsync(
                file,
                ffmpegPath,
                analysisDirectory,
                derivatives,
                derivativeDetails,
                metadata,
                cancellationToken);
        }

        metadata["derivativeDetails"] = derivativeDetails;

        return new ProjectAssetTechnicalAnalysis(
            probe.ExitCode == 0 || !string.IsNullOrWhiteSpace(ocrExtractedText) ? "ok" : "metadata_only",
            probe.ExitCode == 0
                ? string.IsNullOrWhiteSpace(ocrExtractedText)
                    ? "Media technical analysis completed with Stage 23B derivative metadata."
                    : "Media technical analysis and OCR completed with Stage 23B metadata."
                : string.IsNullOrWhiteSpace(ocrExtractedText)
                    ? "Media saved, but ffprobe could not complete. Structured fallback metadata was recorded."
                    : "Media saved and OCR text was extracted, but ffprobe could not complete.",
            ocrExtractedText,
            derivatives,
            metadata);
    }

    private async Task CreateThumbnailAsync(
        StoredProjectAssetFile file,
        string ffmpegPath,
        string analysisDirectory,
        List<string> derivatives,
        List<Dictionary<string, object?>> derivativeDetails,
        Dictionary<string, object?> metadata,
        CancellationToken cancellationToken)
    {
        var thumbnailPath = Path.Combine(analysisDirectory, "thumbnail.jpg");
        var thumbnail = await RunToolAsync(
            ffmpegPath,
            [
                "-y",
                "-hide_banner",
                "-loglevel", "error",
                "-i", file.LocalPath,
                "-frames:v", "1",
                "-vf", "scale=512:-2:force_original_aspect_ratio=decrease",
                thumbnailPath
            ],
            TimeSpan.FromSeconds(Math.Max(10, _options.AssetParseTimeoutSeconds)),
            cancellationToken);

        var created = thumbnail.ExitCode == 0 && File.Exists(thumbnailPath);
        metadata["thumbnail"] = new Dictionary<string, object?>
        {
            ["status"] = created ? "ok" : "failed",
            ["exitCode"] = thumbnail.ExitCode,
            ["path"] = created ? thumbnailPath : null,
            ["error"] = Truncate(thumbnail.Stderr, 4_000)
        };
        AddDerivativeIfExists(derivatives, derivativeDetails, thumbnailPath, "thumbnail", created);
    }

    private async Task CreateImagePreviewAsync(
        StoredProjectAssetFile file,
        string ffmpegPath,
        string analysisDirectory,
        List<string> derivatives,
        List<Dictionary<string, object?>> derivativeDetails,
        Dictionary<string, object?> metadata,
        CancellationToken cancellationToken)
    {
        var previewPath = Path.Combine(analysisDirectory, "preview_1280.jpg");
        var preview = await RunToolAsync(
            ffmpegPath,
            [
                "-y",
                "-hide_banner",
                "-loglevel", "error",
                "-i", file.LocalPath,
                "-vf", $"scale={ImagePreviewMaxWidth}:-2:force_original_aspect_ratio=decrease",
                "-q:v", "5",
                previewPath
            ],
            TimeSpan.FromSeconds(Math.Max(15, _options.AssetParseTimeoutSeconds)),
            cancellationToken);

        var created = preview.ExitCode == 0 && File.Exists(previewPath);
        metadata["imageCompression"] = new Dictionary<string, object?>
        {
            ["status"] = created ? "ok" : "failed",
            ["maxWidth"] = ImagePreviewMaxWidth,
            ["quality"] = 5,
            ["originalPreserved"] = true,
            ["path"] = created ? previewPath : null,
            ["exitCode"] = preview.ExitCode,
            ["error"] = Truncate(preview.Stderr, 4_000)
        };
        AddDerivativeIfExists(derivatives, derivativeDetails, previewPath, "image_preview", created);
    }

    private async Task ExtractVideoFramesAsync(
        StoredProjectAssetFile file,
        string ffmpegPath,
        string probeJson,
        string analysisDirectory,
        List<string> derivatives,
        List<Dictionary<string, object?>> derivativeDetails,
        Dictionary<string, object?> metadata,
        CancellationToken cancellationToken)
    {
        var frameDirectory = Path.Combine(analysisDirectory, "frames");
        Directory.CreateDirectory(frameDirectory);
        var framePattern = Path.Combine(frameDirectory, "frame_%03d.jpg");
        var frameLimit = Math.Clamp(_options.AssetVideoFrameLimit, 1, 8);
        var durationSeconds = TryReadDurationSeconds(probeJson);
        var intervalSeconds = Math.Max(1, (int)Math.Floor((durationSeconds ?? 40) / Math.Max(1, frameLimit)));

        var frames = await RunToolAsync(
            ffmpegPath,
            [
                "-y",
                "-hide_banner",
                "-loglevel", "error",
                "-i", file.LocalPath,
                "-vf", $"fps=1/{intervalSeconds},scale=512:-2:force_original_aspect_ratio=decrease",
                "-frames:v", frameLimit.ToString(CultureInfo.InvariantCulture),
                framePattern
            ],
            TimeSpan.FromSeconds(Math.Max(30, _options.AssetTranscodeTimeoutSeconds)),
            cancellationToken);

        var framePaths = Directory.GetFiles(frameDirectory, "frame_*.jpg").OrderBy(path => path).ToList();
        foreach (var framePath in framePaths)
        {
            AddDerivativeIfExists(derivatives, derivativeDetails, framePath, "video_frame", File.Exists(framePath));
        }

        metadata["frameExtraction"] = new Dictionary<string, object?>
        {
            ["status"] = frames.ExitCode == 0 ? "ok" : "failed",
            ["sampling"] = "evenly_spaced_by_duration",
            ["targetFrameCount"] = frameLimit,
            ["actualFrameCount"] = framePaths.Count,
            ["intervalSeconds"] = intervalSeconds,
            ["durationSeconds"] = durationSeconds,
            ["frameDirectory"] = frameDirectory,
            ["exitCode"] = frames.ExitCode,
            ["error"] = Truncate(frames.Stderr, 4_000)
        };
    }

    private async Task CreateVideoReviewProxyAsync(
        StoredProjectAssetFile file,
        string ffmpegPath,
        string analysisDirectory,
        List<string> derivatives,
        List<Dictionary<string, object?>> derivativeDetails,
        Dictionary<string, object?> metadata,
        CancellationToken cancellationToken)
    {
        var proxyPath = Path.Combine(analysisDirectory, "review_proxy_720p.mp4");
        var proxy = await RunToolAsync(
            ffmpegPath,
            [
                "-y",
                "-hide_banner",
                "-loglevel", "error",
                "-i", file.LocalPath,
                "-t", VideoReviewProxySeconds.ToString(CultureInfo.InvariantCulture),
                "-vf", "scale=720:-2:force_original_aspect_ratio=decrease",
                "-an",
                "-c:v", "libx264",
                "-preset", "veryfast",
                "-crf", "28",
                "-movflags", "+faststart",
                proxyPath
            ],
            TimeSpan.FromSeconds(Math.Max(30, _options.AssetTranscodeTimeoutSeconds)),
            cancellationToken);

        var created = proxy.ExitCode == 0 && File.Exists(proxyPath);
        metadata["videoCompression"] = new Dictionary<string, object?>
        {
            ["status"] = created ? "ok" : "failed",
            ["kind"] = "short_review_proxy",
            ["maxWidth"] = 720,
            ["maxSeconds"] = VideoReviewProxySeconds,
            ["originalPreserved"] = true,
            ["finalExportGenerated"] = false,
            ["path"] = created ? proxyPath : null,
            ["exitCode"] = proxy.ExitCode,
            ["error"] = Truncate(proxy.Stderr, 4_000)
        };
        AddDerivativeIfExists(derivatives, derivativeDetails, proxyPath, "video_review_proxy", created);
    }

    private string ResolveToolPath(string fileName)
    {
        var configured = string.IsNullOrWhiteSpace(_options.FfmpegBinPath)
            ? "D:\\code\\MiLuStudio\\runtime\\ffmpeg\\bin"
            : _options.FfmpegBinPath;
        return Path.Combine(configured, fileName);
    }

    private static Dictionary<string, object?> CreateDocumentMetadata(StoredProjectAssetFile file, string engine)
    {
        return new Dictionary<string, object?>
        {
            ["engine"] = engine,
            ["extension"] = file.Extension,
            ["sourceFile"] = new Dictionary<string, object?>
            {
                ["originalFileName"] = file.OriginalFileName,
                ["fileSize"] = file.FileSize,
                ["sha256"] = file.Sha256
            },
            ["generationPayloadSent"] = false,
            ["modelProviderUsed"] = false
        };
    }

    private static void AddTextManifest(
        Dictionary<string, object?> metadata,
        string text,
        string sourceType,
        string blockKind,
        bool truncated)
    {
        var chunks = BuildTextChunks(text, sourceType);
        metadata["text"] = new Dictionary<string, object?>
        {
            ["sourceType"] = sourceType,
            ["characterCount"] = CountNonWhitespace(text),
            ["rawCharacterCount"] = text.Length,
            ["lineCount"] = CountLines(text),
            ["truncatedToAnalysisLimit"] = truncated,
            ["maxExtractedTextCharacters"] = MaxExtractedTextCharacters
        };
        metadata["contentBlocks"] = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["id"] = "block_0001",
                ["kind"] = blockKind,
                ["sourceType"] = sourceType,
                ["characterCount"] = CountNonWhitespace(text),
                ["lineCount"] = CountLines(text),
                ["chunkCount"] = chunks.Count,
                ["usableAsStoryCandidate"] = true
            }
        };
        metadata["chunkManifest"] = new Dictionary<string, object?>
        {
            ["status"] = "ok",
            ["strategy"] = "stage23b_fixed_character_chunks_v1",
            ["chunkSizeCharacters"] = TextChunkSizeCharacters,
            ["overlapCharacters"] = TextChunkOverlapCharacters,
            ["totalChunks"] = chunks.Count,
            ["chunks"] = chunks
        };
    }

    private static void AddUnavailableChunkManifest(Dictionary<string, object?> metadata, string reason)
    {
        metadata["contentBlocks"] = Array.Empty<object>();
        metadata["chunkManifest"] = new Dictionary<string, object?>
        {
            ["status"] = "unavailable",
            ["reason"] = reason,
            ["strategy"] = "stage23b_fixed_character_chunks_v1",
            ["chunkSizeCharacters"] = TextChunkSizeCharacters,
            ["overlapCharacters"] = TextChunkOverlapCharacters,
            ["totalChunks"] = 0,
            ["chunks"] = Array.Empty<object>()
        };
    }

    private static List<Dictionary<string, object?>> BuildTextChunks(string text, string sourceType)
    {
        var chunks = new List<Dictionary<string, object?>>();
        if (string.IsNullOrEmpty(text))
        {
            return chunks;
        }

        var start = 0;
        var index = 0;
        while (start < text.Length)
        {
            var end = Math.Min(text.Length, start + TextChunkSizeCharacters);
            var content = text[start..end];
            chunks.Add(new Dictionary<string, object?>
            {
                ["id"] = $"chunk_{index + 1:0000}",
                ["index"] = index,
                ["sourceType"] = sourceType,
                ["startCharacter"] = start,
                ["endCharacter"] = end,
                ["characterCount"] = content.Length,
                ["nonWhitespaceCharacterCount"] = CountNonWhitespace(content),
                ["estimatedTokens"] = Math.Max(1, (int)Math.Ceiling(content.Length / 2.0)),
                ["preview"] = Truncate(CollapseWhitespace(content), MaxChunkPreviewCharacters)
            });

            if (end >= text.Length)
            {
                break;
            }

            start = Math.Max(start + 1, end - TextChunkOverlapCharacters);
            index++;
        }

        return chunks;
    }

    private async Task<OcrAnalysisResult> AnalyzeOcrAsync(
        StoredProjectAssetFile file,
        CancellationToken cancellationToken)
    {
        var probe = ResolveOcrRuntime();
        if (!DirectOcrImageExtensions.Contains(file.Extension))
        {
            return new OcrAnalysisResult(
                BuildOcrMetadata(file, probe, "unsupported_extension", invoked: false),
                null,
                "image_ocr_unsupported_extension",
                false);
        }

        if (!probe.RuntimeAvailable || string.IsNullOrWhiteSpace(probe.ExecutablePath))
        {
            return new OcrAnalysisResult(
                BuildOcrMetadata(file, probe, "runtime_not_configured", invoked: false),
                null,
                "image_ocr_runtime_not_configured",
                false);
        }

        return await RunOcrForPathAsync(
            file,
            file.LocalPath,
            probe,
            "image_ocr_runtime_failed_or_no_text",
            cancellationToken);
    }

    private async Task<PdfRasterOcrResult> AnalyzeScannedPdfOcrAsync(
        StoredProjectAssetFile file,
        CancellationToken cancellationToken)
    {
        var rasterizerProbe = ResolvePdfRasterizerRuntime();
        var ocrProbe = ResolveOcrRuntime();
        var rasterizerMetadata = BuildPdfRasterizerMetadata(
            rasterizerProbe,
            ocrProbe,
            rasterizerProbe.RuntimeAvailable ? "ready" : "runtime_not_configured",
            invoked: false);
        var ocrMetadata = BuildOcrMetadata(
            file,
            ocrProbe,
            ocrProbe.RuntimeAvailable ? "waiting_for_pdf_rasterizer" : "runtime_not_configured",
            invoked: false);

        if (!rasterizerProbe.RuntimeAvailable || string.IsNullOrWhiteSpace(rasterizerProbe.ExecutablePath))
        {
            ocrMetadata = BuildOcrMetadata(
                file,
                ocrProbe,
                ocrProbe.RuntimeAvailable ? "pdf_rasterizer_not_configured" : "runtime_not_configured",
                invoked: false);
            return new PdfRasterOcrResult(
                rasterizerMetadata,
                ocrMetadata,
                null,
                "pdf_rasterizer_not_configured",
                false);
        }

        if (!ocrProbe.RuntimeAvailable || string.IsNullOrWhiteSpace(ocrProbe.ExecutablePath))
        {
            rasterizerMetadata["status"] = "ocr_runtime_not_configured";
            return new PdfRasterOcrResult(
                rasterizerMetadata,
                ocrMetadata,
                null,
                "pdf_ocr_runtime_not_configured",
                false);
        }

        var pageLimit = Math.Clamp(_options.PdfRasterizerPageLimit, 1, 12);
        var dpi = Math.Clamp(_options.PdfRasterizerDpi, 72, 300);
        var pageDirectory = Path.Combine(Path.GetDirectoryName(file.LocalPath) ?? ".", "analysis", "pdf-pages");
        Directory.CreateDirectory(pageDirectory);

        var outputPrefix = Path.Combine(pageDirectory, "page");
        var rasterize = await RunToolAsync(
            rasterizerProbe.ExecutablePath,
            [
                "-png",
                "-r", dpi.ToString(CultureInfo.InvariantCulture),
                "-f", "1",
                "-l", pageLimit.ToString(CultureInfo.InvariantCulture),
                file.LocalPath,
                outputPrefix
            ],
            TimeSpan.FromSeconds(Math.Max(10, _options.AssetParseTimeoutSeconds)),
            cancellationToken);

        var pageImages = Directory.GetFiles(pageDirectory, "page-*.png")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Take(pageLimit)
            .ToList();

        rasterizerMetadata["status"] = rasterize.ExitCode == 0 && pageImages.Count > 0 ? "ok" : "failed";
        rasterizerMetadata["invoked"] = true;
        rasterizerMetadata["exitCode"] = rasterize.ExitCode;
        rasterizerMetadata["stderr"] = Truncate(rasterize.Stderr, MaxOcrAttemptErrorCharacters);
        rasterizerMetadata["dpi"] = dpi;
        rasterizerMetadata["pageLimit"] = pageLimit;
        rasterizerMetadata["generatedPageCount"] = pageImages.Count;

        if (pageImages.Count == 0)
        {
            return new PdfRasterOcrResult(
                rasterizerMetadata,
                BuildOcrMetadata(file, ocrProbe, "waiting_for_pdf_rasterizer", invoked: false),
                null,
                "pdf_rasterizer_failed_or_no_pages",
                false);
        }

        var pageSummaries = new List<Dictionary<string, object?>>();
        var textParts = new List<string>();
        var truncated = false;
        for (var index = 0; index < pageImages.Count; index++)
        {
            var pageOcr = await RunOcrForPathAsync(
                file,
                pageImages[index],
                ocrProbe,
                "pdf_page_ocr_runtime_failed_or_no_text",
                cancellationToken);
            pageSummaries.Add(new Dictionary<string, object?>
            {
                ["pageNumber"] = index + 1,
                ["status"] = pageOcr.Metadata.TryGetValue("status", out var status) ? status : "unknown",
                ["language"] = pageOcr.Metadata.TryGetValue("language", out var language) ? language : null,
                ["extractedTextLength"] = pageOcr.ExtractedText?.Length ?? 0,
                ["unavailableReason"] = pageOcr.ExtractedText is null ? pageOcr.UnavailableChunkReason : null
            });

            if (!string.IsNullOrWhiteSpace(pageOcr.ExtractedText))
            {
                textParts.Add($"Page {index + 1}\n{pageOcr.ExtractedText}");
                truncated |= pageOcr.Truncated;
            }
        }

        rasterizerMetadata["pageOcr"] = pageSummaries;
        var combinedText = string.Join("\n\n", textParts).Trim();
        if (!ContainsTextSignal(combinedText))
        {
            rasterizerMetadata["status"] = "no_text";
            var noTextOcrMetadata = BuildOcrMetadata(file, ocrProbe, "runtime_failed_or_no_text", invoked: true);
            noTextOcrMetadata["source"] = "pdf_rasterizer";
            noTextOcrMetadata["pageCount"] = pageImages.Count;
            noTextOcrMetadata["pages"] = pageSummaries;
            return new PdfRasterOcrResult(
                rasterizerMetadata,
                noTextOcrMetadata,
                null,
                "pdf_raster_ocr_text_not_found",
                false);
        }

        var extractedText = Truncate(combinedText, MaxOcrTextCharacters);
        var finalOcrMetadata = BuildOcrMetadata(file, ocrProbe, "ok", invoked: true);
        finalOcrMetadata["source"] = "pdf_rasterizer";
        finalOcrMetadata["pageCount"] = pageImages.Count;
        finalOcrMetadata["extractedTextLength"] = extractedText.Length;
        finalOcrMetadata["truncatedToAnalysisLimit"] = truncated || combinedText.Length > extractedText.Length;
        finalOcrMetadata["pages"] = pageSummaries;

        return new PdfRasterOcrResult(
            rasterizerMetadata,
            finalOcrMetadata,
            extractedText,
            "pdf_raster_ocr_text_not_found",
            truncated || combinedText.Length > extractedText.Length);
    }

    private async Task<OcrAnalysisResult> RunOcrForPathAsync(
        StoredProjectAssetFile file,
        string inputPath,
        OcrRuntimeProbe probe,
        string unavailableChunkReason,
        CancellationToken cancellationToken)
    {
        if (!probe.RuntimeAvailable || string.IsNullOrWhiteSpace(probe.ExecutablePath))
        {
            return new OcrAnalysisResult(
                BuildOcrMetadata(file, probe, "runtime_not_configured", invoked: false),
                null,
                unavailableChunkReason,
                false);
        }

        var executablePath = probe.ExecutablePath;
        var attempts = new List<Dictionary<string, object?>>();
        foreach (var language in probe.Languages)
        {
            var arguments = new List<string>
            {
                inputPath,
                "stdout",
                "--psm",
                "6",
                "-l",
                language
            };

            if (!string.IsNullOrWhiteSpace(probe.TessdataPath))
            {
                arguments.Add("--tessdata-dir");
                arguments.Add(probe.TessdataPath);
            }

            var environment = string.IsNullOrWhiteSpace(probe.TessdataPath)
                ? null
                : new Dictionary<string, string> { ["TESSDATA_PREFIX"] = probe.TessdataPath };
            var result = await RunToolAsync(
                executablePath,
                arguments,
                TimeSpan.FromSeconds(Math.Max(5, _options.OcrTimeoutSeconds)),
                cancellationToken,
                environment);

            var collapsed = CollapseWhitespace(result.Stdout);
            attempts.Add(new Dictionary<string, object?>
            {
                ["language"] = language,
                ["exitCode"] = result.ExitCode,
                ["stdoutLength"] = result.Stdout.Length,
                ["stderr"] = Truncate(result.Stderr, MaxOcrAttemptErrorCharacters)
            });

            if (result.ExitCode == 0 && ContainsTextSignal(collapsed))
            {
                var extractedText = Truncate(collapsed, MaxOcrTextCharacters);
                var metadata = BuildOcrMetadata(file, probe, "ok", invoked: true);
                metadata["language"] = language;
                metadata["attempts"] = attempts;
                metadata["extractedTextLength"] = extractedText.Length;
                metadata["truncatedToAnalysisLimit"] = collapsed.Length > extractedText.Length;
                return new OcrAnalysisResult(
                    metadata,
                    extractedText,
                    "image_ocr_text_not_found",
                    collapsed.Length > extractedText.Length);
            }
        }

        var failedMetadata = BuildOcrMetadata(file, probe, "runtime_failed", invoked: true);
        failedMetadata["attempts"] = attempts;
        return new OcrAnalysisResult(
            failedMetadata,
            null,
            unavailableChunkReason,
            false);
    }

    private Dictionary<string, object?> BuildOcrPlan(
        StoredProjectAssetFile file,
        string statusWhenUnavailable,
        string? statusWhenRuntimeAvailable = null)
    {
        var probe = ResolveOcrRuntime();
        var status = statusWhenUnavailable is "not_required" or "not_applicable"
            ? statusWhenUnavailable
            : probe.RuntimeAvailable
                ? statusWhenRuntimeAvailable ?? "runtime_available_not_invoked_by_default"
                : statusWhenUnavailable;
        return BuildOcrMetadata(file, probe, status, invoked: false);
    }

    private Dictionary<string, object?> BuildOcrMetadata(
        StoredProjectAssetFile file,
        OcrRuntimeProbe probe,
        string status,
        bool invoked)
    {
        return new Dictionary<string, object?>
        {
            ["engine"] = "tesseract_compatible_backend_runtime",
            ["status"] = status,
            ["candidate"] = OcrCandidateExtensions.Contains(file.Extension),
            ["directImageInputSupported"] = DirectOcrImageExtensions.Contains(file.Extension),
            ["pdfRasterizerRequired"] = string.Equals(file.Extension, "pdf", StringComparison.OrdinalIgnoreCase),
            ["runtimeAvailable"] = probe.RuntimeAvailable,
            ["invoked"] = invoked,
            ["checkedPaths"] = probe.CheckedPaths,
            ["languages"] = probe.Languages,
            ["tessdataAvailable"] = probe.TessdataAvailable,
            ["uiElectronFileAccess"] = false,
            ["generationPayloadSent"] = false,
            ["modelProviderUsed"] = false
        };
    }

    private Dictionary<string, object?> BuildPdfRasterizerMetadata(
        PdfRasterizerRuntimeProbe rasterizerProbe,
        OcrRuntimeProbe ocrProbe,
        string status,
        bool invoked)
    {
        return new Dictionary<string, object?>
        {
            ["engine"] = "poppler_pdftoppm_backend_runtime",
            ["status"] = status,
            ["candidate"] = true,
            ["runtimeAvailable"] = rasterizerProbe.RuntimeAvailable,
            ["checkedPaths"] = rasterizerProbe.CheckedPaths,
            ["ocrRuntimeAvailable"] = ocrProbe.RuntimeAvailable,
            ["invoked"] = invoked,
            ["dpi"] = Math.Clamp(_options.PdfRasterizerDpi, 72, 300),
            ["pageLimit"] = Math.Clamp(_options.PdfRasterizerPageLimit, 1, 12),
            ["backendAdapterOnly"] = true,
            ["uiElectronFileAccess"] = false,
            ["generationPayloadSent"] = false,
            ["modelProviderUsed"] = false
        };
    }

    private PdfRasterizerRuntimeProbe ResolvePdfRasterizerRuntime()
    {
        var paths = new List<string>();
        if (!string.IsNullOrWhiteSpace(_options.PdfRasterizerPath))
        {
            paths.Add(_options.PdfRasterizerPath);
        }

        paths.AddRange(DefaultPdfRasterizerCandidatePaths);

        var checkedPaths = paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetFullPath(path.Trim()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => new Dictionary<string, object?>
            {
                ["path"] = path,
                ["exists"] = File.Exists(path)
            })
            .ToList();
        var executablePath = checkedPaths
            .Where(candidate => candidate["exists"] is true)
            .Select(candidate => candidate["path"] as string)
            .FirstOrDefault();

        return new PdfRasterizerRuntimeProbe(
            executablePath is not null,
            executablePath,
            checkedPaths);
    }

    private OcrRuntimeProbe ResolveOcrRuntime()
    {
        var paths = new List<string>();
        if (!string.IsNullOrWhiteSpace(_options.OcrTesseractPath))
        {
            paths.Add(_options.OcrTesseractPath);
        }

        paths.AddRange(DefaultOcrCandidatePaths);

        var checkedPaths = paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetFullPath(path.Trim()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => new Dictionary<string, object?>
            {
                ["path"] = path,
                ["exists"] = File.Exists(path)
            })
            .ToList();
        var executablePath = checkedPaths
            .Where(candidate => candidate["exists"] is true)
            .Select(candidate => candidate["path"] as string)
            .FirstOrDefault();
        var tessdataPath = ResolveTessdataPath(executablePath);
        var languages = ResolveOcrLanguages();

        return new OcrRuntimeProbe(
            executablePath is not null,
            executablePath,
            tessdataPath,
            tessdataPath is not null && Directory.Exists(tessdataPath),
            checkedPaths,
            languages);
    }

    private string? ResolveTessdataPath(string? executablePath)
    {
        if (!string.IsNullOrWhiteSpace(_options.OcrTessdataPath))
        {
            return Path.GetFullPath(_options.OcrTessdataPath.Trim());
        }

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        var sibling = Path.Combine(Path.GetDirectoryName(executablePath) ?? ".", "tessdata");
        return Directory.Exists(sibling) ? sibling : null;
    }

    private IReadOnlyList<string> ResolveOcrLanguages()
    {
        var values = (string.IsNullOrWhiteSpace(_options.OcrLanguages)
                ? DefaultOcrLanguages
                : _options.OcrLanguages.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return values.Count == 0 ? DefaultOcrLanguages : values;
    }

    private static object BuildCompressionPolicy(string kind)
    {
        return new Dictionary<string, object?>
        {
            ["stage"] = "stage23b",
            ["backendAdapterOnly"] = true,
            ["originalPreserved"] = true,
            ["uiElectronFileAccess"] = false,
            ["finalExportGenerated"] = false,
            ["imagePreview"] = kind == "image_reference"
                ? new Dictionary<string, object?>
                {
                    ["enabled"] = true,
                    ["maxWidth"] = ImagePreviewMaxWidth,
                    ["format"] = "jpg",
                    ["quality"] = 5
                }
                : null,
            ["videoReviewProxy"] = kind == "video_reference"
                ? new Dictionary<string, object?>
                {
                    ["enabled"] = true,
                    ["maxWidth"] = 720,
                    ["maxSeconds"] = VideoReviewProxySeconds,
                    ["format"] = "mp4",
                    ["finalExport"] = false
                }
                : null
        };
    }

    private static PdfTextExtractionResult ExtractPdfText(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var raw = TextEncoding.Latin1.GetString(bytes);
        var parts = new List<string>();
        var textObjectCount = AddPdfTextPartsFromSource(parts, raw);
        var streamCount = 0;
        var decodedStreamCount = 0;
        var failedStreamCount = 0;
        var warnings = new List<string>
        {
            "This is a lightweight embedded-text probe. Stage 23C can decode simple Flate streams, but scanned pages and complex encodings still require backend PDF/OCR runtime support."
        };

        foreach (global::System.Text.RegularExpressions.Match match in PdfStreamRegex.Matches(raw).Take(MaxPdfStreamDecodeAttempts))
        {
            streamCount++;
            var dictionary = match.Groups["dictionary"].Value;
            if (!PdfFlateDecodeRegex.IsMatch(dictionary))
            {
                continue;
            }

            var streamBytes = PreparePdfStreamBytes(match.Groups["body"].Value, dictionary);
            if (TryDecodePdfFlateStream(streamBytes, out var decoded, out var failureMessage))
            {
                decodedStreamCount++;
                textObjectCount += AddPdfTextPartsFromSource(parts, decoded);
            }
            else
            {
                failedStreamCount++;
                warnings.Add($"Flate stream decode failed: {failureMessage}");
            }
        }

        var totalStreams = PdfStreamRegex.Matches(raw).Count;
        streamCount = Math.Max(streamCount, totalStreams);
        if (totalStreams > MaxPdfStreamDecodeAttempts)
        {
            warnings.Add($"Only the first {MaxPdfStreamDecodeAttempts} PDF streams were considered for lightweight decoding.");
        }

        var text = string.Join("\n", parts.Distinct(StringComparer.Ordinal)).Trim();
        if (CountNonWhitespace(text) < 20)
        {
            warnings.Add("No usable embedded text was found; OCR should be scheduled when runtime support is available.");
            text = string.Empty;
        }

        return new PdfTextExtractionResult(text, textObjectCount, streamCount, decodedStreamCount, failedStreamCount, warnings);
    }

    private static int AddPdfTextPartsFromSource(List<string> parts, string source)
    {
        var textObjectCount = 0;
        foreach (global::System.Text.RegularExpressions.Match match in PdfLiteralTextRegex.Matches(source))
        {
            textObjectCount++;
            AddPdfTextPart(parts, DecodePdfLiteralString(match.Groups["text"].Value));
        }

        foreach (global::System.Text.RegularExpressions.Match match in PdfHexTextRegex.Matches(source))
        {
            textObjectCount++;
            AddPdfTextPart(parts, DecodePdfHexString(match.Groups["hex"].Value));
        }

        foreach (global::System.Text.RegularExpressions.Match match in PdfArrayTextRegex.Matches(source))
        {
            textObjectCount++;
            var array = match.Groups["array"].Value;
            foreach (global::System.Text.RegularExpressions.Match literal in PdfArrayLiteralRegex.Matches(array))
            {
                AddPdfTextPart(parts, DecodePdfLiteralString(literal.Groups["text"].Value));
            }

            foreach (global::System.Text.RegularExpressions.Match hex in PdfHexValueRegex.Matches(array))
            {
                AddPdfTextPart(parts, DecodePdfHexString(hex.Groups["hex"].Value));
            }
        }

        return textObjectCount;
    }

    private static byte[] PreparePdfStreamBytes(string streamBody, string dictionary)
    {
        var body = RemovePdfStreamBoundaryNewlines(streamBody);
        var bytes = TextEncoding.Latin1.GetBytes(body);
        var length = TryReadPdfStreamLength(dictionary);
        if (length is > 0 && length.Value < bytes.Length)
        {
            return bytes.Take(length.Value).ToArray();
        }

        return bytes;
    }

    private static string RemovePdfStreamBoundaryNewlines(string value)
    {
        if (value.StartsWith("\r\n", StringComparison.Ordinal))
        {
            value = value[2..];
        }
        else if (value.StartsWith('\n') || value.StartsWith('\r'))
        {
            value = value[1..];
        }

        if (value.EndsWith("\r\n", StringComparison.Ordinal))
        {
            return value[..^2];
        }

        if (value.EndsWith('\n') || value.EndsWith('\r'))
        {
            return value[..^1];
        }

        return value;
    }

    private static int? TryReadPdfStreamLength(string dictionary)
    {
        var match = PdfStreamLengthRegex.Match(dictionary);
        if (!match.Success)
        {
            return null;
        }

        return int.TryParse(match.Groups["length"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var length)
            ? length
            : null;
    }

    private static bool TryDecodePdfFlateStream(byte[] streamBytes, out string decoded, out string failureMessage)
    {
        if (TryInflatePdfStream(streamBytes, useZLibHeader: true, out decoded, out failureMessage))
        {
            return true;
        }

        return TryInflatePdfStream(streamBytes, useZLibHeader: false, out decoded, out failureMessage);
    }

    private static bool TryInflatePdfStream(
        byte[] streamBytes,
        bool useZLibHeader,
        out string decoded,
        out string failureMessage)
    {
        decoded = string.Empty;
        failureMessage = string.Empty;

        try
        {
            using var input = new MemoryStream(streamBytes);
            using var output = new MemoryStream();
            using var inflater = useZLibHeader
                ? (Stream)new global::System.IO.Compression.ZLibStream(input, global::System.IO.Compression.CompressionMode.Decompress)
                : new global::System.IO.Compression.DeflateStream(input, global::System.IO.Compression.CompressionMode.Decompress);
            var buffer = new byte[8192];
            while (true)
            {
                var read = inflater.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    break;
                }

                output.Write(buffer, 0, read);
                if (output.Length > MaxPdfDecodedStreamBytes)
                {
                    failureMessage = $"decoded stream exceeded {MaxPdfDecodedStreamBytes} bytes";
                    return false;
                }
            }

            decoded = TextEncoding.Latin1.GetString(output.ToArray());
            return decoded.Length > 0;
        }
        catch (Exception error) when (error is InvalidDataException or IOException)
        {
            failureMessage = error.Message;
            return false;
        }
    }

    private static void AddPdfTextPart(List<string> parts, string value)
    {
        var sanitized = CollapseWhitespace(value);
        if (sanitized.Length > 0 && ContainsTextSignal(sanitized))
        {
            parts.Add(sanitized);
        }
    }

    private static string DecodePdfLiteralString(string value)
    {
        var builder = new global::System.Text.StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];
            if (current != '\\' || index + 1 >= value.Length)
            {
                builder.Append(current);
                continue;
            }

            var next = value[++index];
            switch (next)
            {
                case 'n':
                    builder.Append('\n');
                    break;
                case 'r':
                    builder.Append('\r');
                    break;
                case 't':
                    builder.Append('\t');
                    break;
                case 'b':
                    builder.Append('\b');
                    break;
                case 'f':
                    builder.Append('\f');
                    break;
                case '(':
                case ')':
                case '\\':
                    builder.Append(next);
                    break;
                case '\r':
                    if (index + 1 < value.Length && value[index + 1] == '\n')
                    {
                        index++;
                    }
                    break;
                case '\n':
                    break;
                default:
                    if (next is >= '0' and <= '7')
                    {
                        var octal = new string(next, 1);
                        for (var octalIndex = 0; octalIndex < 2 && index + 1 < value.Length && value[index + 1] is >= '0' and <= '7'; octalIndex++)
                        {
                            octal += value[++index];
                        }

                        builder.Append((char)Convert.ToInt32(octal, 8));
                    }
                    else
                    {
                        builder.Append(next);
                    }
                    break;
            }
        }

        return builder.ToString();
    }

    private static string DecodePdfHexString(string hex)
    {
        var compact = Regex.Replace(hex, @"\s+", string.Empty);
        if (compact.Length < 2)
        {
            return string.Empty;
        }

        if (compact.Length % 2 == 1)
        {
            compact += "0";
        }

        var bytes = new byte[compact.Length / 2];
        try
        {
            for (var index = 0; index < bytes.Length; index++)
            {
                bytes[index] = Convert.ToByte(compact.Substring(index * 2, 2), 16);
            }
        }
        catch (FormatException)
        {
            return string.Empty;
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return TextEncoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
        }

        return TextEncoding.Latin1.GetString(bytes);
    }

    private static async Task<ToolResult> RunToolAsync(
        string executable,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string>? environment = null)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (environment is not null)
        {
            foreach (var item in environment)
            {
                startInfo.Environment[item.Key] = item.Value;
            }
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start {executable}.");
        var stdout = process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
            return new ToolResult(process.ExitCode, await stdout, await stderr);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            return new ToolResult(-1, string.Empty, $"Tool timed out after {timeout.TotalSeconds:0}s.");
        }
    }

    private static async Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, useAsync: true);
        using var reader = new StreamReader(stream, TextEncoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static DocxExtractionResult ExtractDocxText(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var entries = archive.Entries
            .Where(entry => IsDocxStructuredTextPart(entry.FullName.Replace('\\', '/')))
            .OrderBy(entry => GetDocxPartSortKey(entry.FullName.Replace('\\', '/')))
            .ThenBy(entry => entry.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var blocks = new List<DocxTextBlock>();
        var sourceParts = new List<Dictionary<string, object?>>();
        var warnings = new List<string>();

        foreach (var entry in entries)
        {
            var normalizedName = entry.FullName.Replace('\\', '/');
            var partType = GetDocxPartType(normalizedName);
            var beforeCount = blocks.Count;
            using var entryStream = entry.Open();
            var document = XDocument.Load(entryStream);

            if (partType == "document")
            {
                AddDocxDocumentBlocks(document, normalizedName, blocks, warnings);
            }
            else if (partType is "header" or "footer")
            {
                AddDocxHeaderFooterBlocks(document, normalizedName, partType, blocks);
            }
            else
            {
                AddDocxNoteBlocks(document, normalizedName, partType, blocks);
            }

            sourceParts.Add(new Dictionary<string, object?>
            {
                ["name"] = normalizedName,
                ["type"] = partType,
                ["blockCount"] = blocks.Count - beforeCount
            });
        }

        var text = string.Join(
                "\n\n",
                blocks.Select(block => block.Text).Where(part => !string.IsNullOrWhiteSpace(part)))
            .Trim();
        var preview = blocks
            .Take(MaxDocxStructurePreviewBlocks)
            .Select((block, index) => new Dictionary<string, object?>
            {
                ["id"] = $"docx_block_{index + 1:0000}",
                ["kind"] = block.Kind,
                ["sourcePart"] = block.SourcePart,
                ["style"] = block.Style,
                ["rowCount"] = block.RowCount,
                ["cellCount"] = block.CellCount,
                ["characterCount"] = CountNonWhitespace(block.Text),
                ["preview"] = Truncate(CollapseWhitespace(block.Text), MaxDocxBlockPreviewCharacters)
            })
            .ToList();
        var structure = new Dictionary<string, object?>
        {
            ["engine"] = "stage23c_docx_zip_xml_structured_reader",
            ["status"] = string.IsNullOrWhiteSpace(text) ? "empty_text" : "ok",
            ["sourcePartCount"] = sourceParts.Count,
            ["sourceParts"] = sourceParts,
            ["blockCount"] = blocks.Count,
            ["paragraphCount"] = blocks.Count(block => block.Kind is "paragraph" or "header" or "footer" or "footnote" or "endnote" or "comment"),
            ["tableCount"] = blocks.Count(block => block.Kind == "table"),
            ["tableCellCount"] = blocks.Sum(block => block.CellCount ?? 0),
            ["headerFooterBlockCount"] = blocks.Count(block => block.Kind is "header" or "footer"),
            ["noteBlockCount"] = blocks.Count(block => block.Kind is "footnote" or "endnote" or "comment"),
            ["previewBlocks"] = preview,
            ["warnings"] = warnings.Distinct(StringComparer.OrdinalIgnoreCase).Take(20).ToList(),
            ["backendAdapterOnly"] = true,
            ["uiElectronFileAccess"] = false,
            ["generationPayloadSent"] = false,
            ["modelProviderUsed"] = false
        };

        return new DocxExtractionResult(text, structure);
    }

    private static bool IsDocxStructuredTextPart(string normalizedName)
    {
        return normalizedName is "word/document.xml"
            || normalizedName.StartsWith("word/header", StringComparison.OrdinalIgnoreCase)
            || normalizedName.StartsWith("word/footer", StringComparison.OrdinalIgnoreCase)
            || normalizedName is "word/footnotes.xml"
            || normalizedName is "word/endnotes.xml"
            || normalizedName is "word/comments.xml";
    }

    private static int GetDocxPartSortKey(string normalizedName)
    {
        if (normalizedName is "word/document.xml")
        {
            return 0;
        }

        if (normalizedName.StartsWith("word/header", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (normalizedName.StartsWith("word/footer", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (normalizedName is "word/footnotes.xml")
        {
            return 3;
        }

        if (normalizedName is "word/endnotes.xml")
        {
            return 4;
        }

        return 5;
    }

    private static string GetDocxPartType(string normalizedName)
    {
        if (normalizedName is "word/document.xml")
        {
            return "document";
        }

        if (normalizedName.StartsWith("word/header", StringComparison.OrdinalIgnoreCase))
        {
            return "header";
        }

        if (normalizedName.StartsWith("word/footer", StringComparison.OrdinalIgnoreCase))
        {
            return "footer";
        }

        if (normalizedName is "word/footnotes.xml")
        {
            return "footnote";
        }

        if (normalizedName is "word/endnotes.xml")
        {
            return "endnote";
        }

        return "comment";
    }

    private static void AddDocxDocumentBlocks(
        XDocument document,
        string sourcePart,
        List<DocxTextBlock> blocks,
        List<string> warnings)
    {
        var body = document.Descendants().FirstOrDefault(element => element.Name.LocalName == "body");
        if (body is null)
        {
            warnings.Add("word/document.xml did not expose a WordprocessingML body element; paragraph fallback was used.");
            AddDocxParagraphBlocks(document.Descendants().Where(element => element.Name.LocalName == "p"), sourcePart, "paragraph", blocks);
            return;
        }

        foreach (var element in body.Elements())
        {
            if (element.Name.LocalName == "p")
            {
                AddDocxParagraphBlock(element, sourcePart, "paragraph", blocks);
            }
            else if (element.Name.LocalName == "tbl")
            {
                AddDocxTableBlock(element, sourcePart, blocks);
            }
        }
    }

    private static void AddDocxHeaderFooterBlocks(
        XDocument document,
        string sourcePart,
        string kind,
        List<DocxTextBlock> blocks)
    {
        AddDocxParagraphBlocks(
            document.Descendants().Where(element => element.Name.LocalName == "p" && !HasDocxAncestor(element, "tbl")),
            sourcePart,
            kind,
            blocks);

        foreach (var table in document.Descendants().Where(element => element.Name.LocalName == "tbl"))
        {
            AddDocxTableBlock(table, sourcePart, blocks);
        }
    }

    private static void AddDocxNoteBlocks(
        XDocument document,
        string sourcePart,
        string kind,
        List<DocxTextBlock> blocks)
    {
        var containers = document.Descendants().Where(element => element.Name.LocalName == kind);
        foreach (var container in containers)
        {
            var text = CollapseWhitespace(ExtractDocxElementText(container));
            if (text.Length > 0)
            {
                blocks.Add(new DocxTextBlock(kind, sourcePart, null, text, null, null));
            }
        }
    }

    private static void AddDocxParagraphBlocks(
        IEnumerable<XElement> paragraphs,
        string sourcePart,
        string kind,
        List<DocxTextBlock> blocks)
    {
        foreach (var paragraph in paragraphs)
        {
            AddDocxParagraphBlock(paragraph, sourcePart, kind, blocks);
        }
    }

    private static void AddDocxParagraphBlock(
        XElement paragraph,
        string sourcePart,
        string kind,
        List<DocxTextBlock> blocks)
    {
        var text = CollapseWhitespace(ExtractDocxElementText(paragraph));
        if (text.Length == 0)
        {
            return;
        }

        blocks.Add(new DocxTextBlock(kind, sourcePart, GetDocxParagraphStyle(paragraph), text, null, null));
    }

    private static void AddDocxTableBlock(XElement table, string sourcePart, List<DocxTextBlock> blocks)
    {
        var rows = table
            .Elements()
            .Where(element => element.Name.LocalName == "tr")
            .Select(row => row
                .Elements()
                .Where(cell => cell.Name.LocalName == "tc")
                .Select(cell => CollapseWhitespace(ExtractDocxElementText(cell)))
                .Where(text => text.Length > 0)
                .ToList())
            .Where(cells => cells.Count > 0)
            .ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var text = string.Join("\n", rows.Select(cells => string.Join(" | ", cells)));
        blocks.Add(new DocxTextBlock(
            "table",
            sourcePart,
            null,
            text,
            rows.Count,
            rows.Sum(cells => cells.Count)));
    }

    private static string? GetDocxParagraphStyle(XElement paragraph)
    {
        var style = paragraph
            .Elements()
            .FirstOrDefault(element => element.Name.LocalName == "pPr")
            ?.Elements()
            .FirstOrDefault(element => element.Name.LocalName == "pStyle");
        var value = style?.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == "val")?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string ExtractDocxElementText(XElement element)
    {
        var builder = new global::System.Text.StringBuilder();
        foreach (var descendant in element.Descendants())
        {
            switch (descendant.Name.LocalName)
            {
                case "t":
                    builder.Append(descendant.Value);
                    break;
                case "tab":
                    builder.Append('\t');
                    break;
                case "br":
                case "cr":
                    builder.Append('\n');
                    break;
            }
        }

        return builder.ToString();
    }

    private static bool HasDocxAncestor(XElement? element, string ancestorLocalName, XElement? stopAt = null)
    {
        for (var current = element?.Parent; current is not null && current != stopAt; current = current.Parent)
        {
            if (current.Name.LocalName == ancestorLocalName)
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, object?> BuildProbeSummary(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, object?> { ["status"] = "unavailable" };
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var summary = new Dictionary<string, object?> { ["status"] = "ok" };

            if (root.TryGetProperty("format", out var format))
            {
                summary["format"] = new Dictionary<string, object?>
                {
                    ["formatName"] = ReadString(format, "format_name"),
                    ["durationSeconds"] = ReadDouble(format, "duration"),
                    ["sizeBytes"] = ReadLong(format, "size"),
                    ["bitRate"] = ReadLong(format, "bit_rate")
                };
            }

            var streams = new List<Dictionary<string, object?>>();
            if (root.TryGetProperty("streams", out var streamItems) && streamItems.ValueKind == JsonValueKind.Array)
            {
                foreach (var stream in streamItems.EnumerateArray())
                {
                    streams.Add(new Dictionary<string, object?>
                    {
                        ["index"] = ReadLong(stream, "index"),
                        ["codecType"] = ReadString(stream, "codec_type"),
                        ["codecName"] = ReadString(stream, "codec_name"),
                        ["width"] = ReadLong(stream, "width"),
                        ["height"] = ReadLong(stream, "height"),
                        ["durationSeconds"] = ReadDouble(stream, "duration"),
                        ["averageFrameRate"] = ParseFrameRate(ReadString(stream, "avg_frame_rate"))
                    });
                }
            }

            summary["streams"] = streams;
            return summary;
        }
        catch (JsonException error)
        {
            return new Dictionary<string, object?>
            {
                ["status"] = "parse_failed",
                ["message"] = Truncate(error.Message, 500)
            };
        }
    }

    private static double? TryReadDurationSeconds(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("format", out var format))
            {
                return ReadDouble(format, "duration");
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()
            : null;
    }

    private static long? ReadLong(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var longValue))
        {
            return longValue;
        }

        return long.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static double? ReadDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var doubleValue))
        {
            return doubleValue;
        }

        return double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static double? ParseFrameRate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "0/0")
        {
            return null;
        }

        var parts = value.Split('/', 2);
        if (parts.Length == 2 &&
            double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var numerator) &&
            double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var denominator) &&
            denominator > 0)
        {
            return numerator / denominator;
        }

        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static void AddDerivativeIfExists(
        List<string> derivatives,
        List<Dictionary<string, object?>> derivativeDetails,
        string path,
        string kind,
        bool created)
    {
        if (!created || !File.Exists(path))
        {
            return;
        }

        derivatives.Add(path);
        var info = new FileInfo(path);
        derivativeDetails.Add(new Dictionary<string, object?>
        {
            ["kind"] = kind,
            ["path"] = path,
            ["fileSize"] = info.Length,
            ["createdBy"] = "stage23b_backend_adapter"
        });
    }

    private static int CountNonWhitespace(string value)
    {
        return value.Count(character => !char.IsWhiteSpace(character));
    }

    private static int CountLines(string value)
    {
        return string.IsNullOrEmpty(value) ? 0 : value.Split('\n').Length;
    }

    private static bool ContainsTextSignal(string value)
    {
        return value.Count(char.IsLetterOrDigit) >= 4;
    }

    private static string CollapseWhitespace(string value)
    {
        return Regex.Replace(value.Replace('\0', ' '), @"\s+", " ").Trim();
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private sealed record DocxExtractionResult(string Text, Dictionary<string, object?> StructureMetadata);

    private sealed record DocxTextBlock(
        string Kind,
        string SourcePart,
        string? Style,
        string Text,
        int? RowCount,
        int? CellCount);

    private sealed record PdfTextExtractionResult(
        string Text,
        int TextObjectCount,
        int StreamCount,
        int DecodedStreamCount,
        int FailedStreamCount,
        IReadOnlyList<string> Warnings);

    private sealed record PdfRasterizerRuntimeProbe(
        bool RuntimeAvailable,
        string? ExecutablePath,
        IReadOnlyList<Dictionary<string, object?>> CheckedPaths);

    private sealed record PdfRasterOcrResult(
        Dictionary<string, object?> RasterizerMetadata,
        Dictionary<string, object?> OcrMetadata,
        string? ExtractedText,
        string UnavailableChunkReason,
        bool Truncated);

    private sealed record OcrRuntimeProbe(
        bool RuntimeAvailable,
        string? ExecutablePath,
        string? TessdataPath,
        bool TessdataAvailable,
        IReadOnlyList<Dictionary<string, object?>> CheckedPaths,
        IReadOnlyList<string> Languages);

    private sealed record OcrAnalysisResult(
        Dictionary<string, object?> Metadata,
        string? ExtractedText,
        string UnavailableChunkReason,
        bool Truncated);

    private sealed record ToolResult(int ExitCode, string Stdout, string Stderr);
}
