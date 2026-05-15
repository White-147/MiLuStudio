namespace MiLuStudio.Application.Assets;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain.Entities;
using JsonSerializer = global::System.Text.Json.JsonSerializer;
using JsonSerializerOptions = global::System.Text.Json.JsonSerializerOptions;

public sealed class ProjectAssetUploadService
{
    public const long MaxTextBytes = 50L * 1024 * 1024;
    public const long MaxImageBytes = 50L * 1024 * 1024;
    public const long MaxVideoBytes = 1024L * 1024 * 1024;
    public const long MinUploadChunkBytes = 1024L * 1024;
    public const long PreferredUploadChunkBytes = 8L * 1024 * 1024;
    public const long MaxUploadChunkBytes = 16L * 1024 * 1024;

    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "txt", "md", "markdown", "csv", "json", "srt", "ass", "vtt", "log", "xml", "yaml", "yml", "rtf", "docx", "doc", "pdf"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "png", "jpg", "jpeg", "webp", "gif", "bmp", "tif", "tiff", "avif", "heic", "heif"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "mp4", "mov", "webm", "mkv", "avi", "m4v", "wmv", "flv", "mpeg", "mpg", "ts", "m2ts", "3gp"
    };

    private readonly IAssetRepository _assets;
    private readonly IAssetTechnicalAnalyzer _analyzer;
    private readonly IClock _clock;
    private readonly IProjectAssetFileStore _fileStore;
    private readonly IProjectRepository _projects;

    public ProjectAssetUploadService(
        IProjectRepository projects,
        IAssetRepository assets,
        IProjectAssetFileStore fileStore,
        IAssetTechnicalAnalyzer analyzer,
        IClock clock)
    {
        _projects = projects;
        _assets = assets;
        _fileStore = fileStore;
        _analyzer = analyzer;
        _clock = clock;
    }

    public async Task<ProjectAssetUploadResponse?> UploadAsync(
        string projectId,
        ProjectAssetUploadRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(projectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var kind = ClassifyKind(request.OriginalFileName, request.ContentType, request.Intent);
        ValidateSize(kind, request.FileSize, request.OriginalFileName);

        var assetId = CreateId("asset");
        var stored = await _fileStore.SaveAsync(
            new ProjectAssetFileSaveRequest(
                projectId,
                assetId,
                request.OriginalFileName,
                request.ContentType,
                request.FileSize,
                request.Content),
            cancellationToken);

        var analysis = await _analyzer.AnalyzeAsync(stored, kind, cancellationToken);
        var now = _clock.Now;
        var metadata = BuildMetadata(request, stored, kind, analysis);
        var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = false });

        var asset = new Asset
        {
            Id = assetId,
            ProjectId = projectId,
            Kind = kind,
            LocalPath = stored.LocalPath,
            MimeType = NormalizeContentType(request.ContentType),
            FileSize = stored.FileSize,
            Sha256 = stored.Sha256,
            MetadataJson = metadataJson,
            CreatedAt = now
        };

        await _assets.AddAsync(asset, cancellationToken);

        return new ProjectAssetUploadResponse(
            asset.Id,
            asset.ProjectId,
            asset.Kind,
            stored.OriginalFileName,
            asset.LocalPath,
            asset.MimeType,
            asset.FileSize,
            asset.Sha256 ?? string.Empty,
            metadataJson,
            now.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            analysis.ExtractedText,
            analysis.Message);
    }

    public static string ClassifyKind(string fileName, string contentType, string? intent)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        var normalizedIntent = intent?.Trim();

        if (string.Equals(normalizedIntent, "storyText", StringComparison.OrdinalIgnoreCase))
        {
            return "story_text";
        }

        if (string.Equals(normalizedIntent, "imageReference", StringComparison.OrdinalIgnoreCase))
        {
            return "image_reference";
        }

        if (string.Equals(normalizedIntent, "videoReference", StringComparison.OrdinalIgnoreCase))
        {
            return "video_reference";
        }

        if (ImageExtensions.Contains(extension) || contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return "image_reference";
        }

        if (VideoExtensions.Contains(extension) || contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return "video_reference";
        }

        if (TextExtensions.Contains(extension) || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
        {
            return "story_text";
        }

        return "reference";
    }

    public static void ValidateSize(string kind, long fileSize, string fileName)
    {
        var limit = kind switch
        {
            "story_text" => MaxTextBytes,
            "image_reference" => MaxImageBytes,
            "video_reference" => MaxVideoBytes,
            _ => MaxTextBytes
        };

        if (fileSize <= 0)
        {
            throw new ProjectAssetUploadException($"{fileName} is empty and cannot be uploaded for analysis.");
        }

        if (fileSize > limit)
        {
            throw new ProjectAssetUploadException($"{fileName} exceeds the current {FormatBytes(limit)} upload limit.");
        }
    }

    private static object BuildMetadata(
        ProjectAssetUploadRequest request,
        StoredProjectAssetFile stored,
        string kind,
        ProjectAssetTechnicalAnalysis analysis)
    {
        return new
        {
            stage = "stage23b_document_media_analysis",
            analysisSchemaVersion = "stage23b_asset_analysis_v1",
            originalFileName = stored.OriginalFileName,
            extension = stored.Extension,
            intent = request.Intent,
            kind,
            sourceMimeType = NormalizeContentType(request.ContentType),
            sha256 = stored.Sha256,
            upload = new
            {
                mode = string.IsNullOrWhiteSpace(request.UploadMode) ? "control_api_multipart" : request.UploadMode,
                uiElectronFileAccess = false,
                chunkingPolicy = new
                {
                    status = "endpoint_available",
                    strategy = "stage23b_resumable_upload_contract_v1",
                    preferredChunkBytes = PreferredUploadChunkBytes,
                    minChunkBytes = MinUploadChunkBytes,
                    maxChunkBytes = MaxUploadChunkBytes,
                    mergeBoundary = "backend_application_service",
                    directFilesystemAccessFromUi = false,
                    endpoint = "/api/projects/{projectId}/assets/upload-sessions"
                }
            },
            parse = new
            {
                analysis.Status,
                analysis.Message,
                extractedTextLength = analysis.ExtractedText?.Length ?? 0,
                generationPayloadSent = false,
                modelProviderUsed = false
            },
            productionInput = new
            {
                storyTextCandidate = kind == "story_text" && !string.IsNullOrWhiteSpace(analysis.ExtractedText)
                    ? analysis.ExtractedText
                    : null,
                source = "asset_analysis_metadata",
                usableAsStoryCandidate = kind == "story_text" && !string.IsNullOrWhiteSpace(analysis.ExtractedText),
                imageReferenceCandidate = kind == "image_reference"
                    ? BuildReferenceProductionCandidate(kind, analysis)
                    : null,
                videoReferenceCandidate = kind == "video_reference"
                    ? BuildReferenceProductionCandidate(kind, analysis)
                    : null,
                usableAsImageReference = kind == "image_reference",
                usableAsVideoReference = kind == "video_reference",
                mediaAccessPolicy = "backend_adapter_only",
                uiElectronFileAccess = false,
                generationPayloadSent = false,
                modelProviderUsed = false
            },
            technical = analysis.Metadata,
            derivatives = analysis.DerivativePaths,
            limits = new
            {
                maxTextBytes = MaxTextBytes,
                maxImageBytes = MaxImageBytes,
                maxVideoBytes = MaxVideoBytes,
                chunkingPolicyRecorded = true,
                imageCompressionPolicyRecorded = true,
                videoCompressionPolicyRecorded = true
            }
        };
    }

    private static object BuildReferenceProductionCandidate(
        string kind,
        ProjectAssetTechnicalAnalysis analysis)
    {
        return new
        {
            kind,
            source = "asset_analysis_media_metadata",
            parseStatus = analysis.Status,
            extractedTextLength = analysis.ExtractedText?.Length ?? 0,
            derivativeSummary = BuildDerivativeProductionSummary(analysis),
            probeSummary = GetMetadataObject(analysis.Metadata, "probeSummary"),
            compressionPolicy = GetMetadataObject(analysis.Metadata, "compressionPolicy"),
            ocr = BuildOcrProductionSummary(analysis),
            thumbnail = BuildOperationProductionSummary(analysis, "thumbnail"),
            imagePreview = BuildOperationProductionSummary(analysis, "imagePreview"),
            frameExtraction = BuildFrameExtractionProductionSummary(analysis),
            videoReviewProxy = BuildOperationProductionSummary(analysis, "videoReviewProxy"),
            mediaAccessPolicy = "backend_adapter_only",
            uiElectronFileAccess = false,
            generationPayloadSent = false,
            modelProviderUsed = false
        };
    }

    private static object BuildDerivativeProductionSummary(ProjectAssetTechnicalAnalysis analysis)
    {
        var kinds = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        if (analysis.Metadata.TryGetValue("derivativeDetails", out var details) &&
            details is IEnumerable<Dictionary<string, object?>> derivativeDetails)
        {
            foreach (var detail in derivativeDetails)
            {
                if (detail.TryGetValue("kind", out var kindValue) && kindValue is string kind && !string.IsNullOrWhiteSpace(kind))
                {
                    kinds.Add(kind);
                }
            }
        }

        return new
        {
            count = Math.Max(analysis.DerivativePaths.Count, kinds.Count),
            kinds = kinds.ToArray(),
            accessPolicy = "backend_adapter_only",
            localPathsExposed = false
        };
    }

    private static object BuildOcrProductionSummary(ProjectAssetTechnicalAnalysis analysis)
    {
        var ocr = GetMetadataObject(analysis.Metadata, "ocr");
        return new
        {
            status = GetString(ocr, "status") ?? "not_recorded",
            candidate = GetBool(ocr, "candidate") ?? false,
            invoked = GetBool(ocr, "invoked") ?? false,
            language = GetString(ocr, "language"),
            extractedTextLength = analysis.ExtractedText?.Length ?? GetInt(ocr, "extractedTextLength") ?? 0,
            uiElectronFileAccess = GetBool(ocr, "uiElectronFileAccess") ?? false,
            modelProviderUsed = GetBool(ocr, "modelProviderUsed") ?? false
        };
    }

    private static object? BuildOperationProductionSummary(
        ProjectAssetTechnicalAnalysis analysis,
        string key)
    {
        var operation = GetMetadataObject(analysis.Metadata, key);
        if (operation is null)
        {
            return null;
        }

        return new
        {
            status = GetString(operation, "status") ?? "not_recorded",
            generated = string.Equals(GetString(operation, "status"), "ok", StringComparison.OrdinalIgnoreCase),
            kind = GetString(operation, "kind"),
            maxWidth = GetInt(operation, "maxWidth"),
            maxSeconds = GetInt(operation, "maxSeconds"),
            originalPreserved = GetBool(operation, "originalPreserved"),
            finalExportGenerated = GetBool(operation, "finalExportGenerated"),
            localPathExposed = false
        };
    }

    private static object? BuildFrameExtractionProductionSummary(ProjectAssetTechnicalAnalysis analysis)
    {
        var frameExtraction = GetMetadataObject(analysis.Metadata, "frameExtraction");
        if (frameExtraction is null)
        {
            return null;
        }

        return new
        {
            status = GetString(frameExtraction, "status") ?? "not_recorded",
            sampling = GetString(frameExtraction, "sampling"),
            targetFrameCount = GetInt(frameExtraction, "targetFrameCount"),
            actualFrameCount = GetInt(frameExtraction, "actualFrameCount") ?? 0,
            intervalSeconds = GetInt(frameExtraction, "intervalSeconds"),
            durationSeconds = GetDouble(frameExtraction, "durationSeconds"),
            localFrameDirectoryExposed = false
        };
    }

    private static IReadOnlyDictionary<string, object?>? GetMetadataObject(
        IReadOnlyDictionary<string, object?> source,
        string key)
    {
        return source.TryGetValue(key, out var value)
            ? value as IReadOnlyDictionary<string, object?>
            : null;
    }

    private static string? GetString(IReadOnlyDictionary<string, object?>? source, string key)
    {
        return source is not null &&
            source.TryGetValue(key, out var value) &&
            value is string text &&
            !string.IsNullOrWhiteSpace(text)
                ? text
                : null;
    }

    private static int? GetInt(IReadOnlyDictionary<string, object?>? source, string key)
    {
        if (source is null || !source.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            int typed => typed,
            long typed => checked((int)typed),
            double typed => (int)typed,
            _ => null
        };
    }

    private static double? GetDouble(IReadOnlyDictionary<string, object?>? source, string key)
    {
        if (source is null || !source.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            double typed => typed,
            int typed => typed,
            long typed => typed,
            _ => null
        };
    }

    private static bool? GetBool(IReadOnlyDictionary<string, object?>? source, string key)
    {
        return source is not null &&
            source.TryGetValue(key, out var value) &&
            value is bool typed
                ? typed
                : null;
    }

    private static string NormalizeContentType(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "application/octet-stream" : value.Trim();
    }

    private static string FormatBytes(long bytes)
    {
        return bytes >= 1024L * 1024 * 1024
            ? $"{bytes / 1024d / 1024d / 1024d:0.#}GB"
            : $"{bytes / 1024d / 1024d:0.#}MB";
    }

    private static string CreateId(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }
}

public sealed class ProjectAssetUploadException : Exception
{
    public ProjectAssetUploadException(string message)
        : base(message)
    {
    }
}
