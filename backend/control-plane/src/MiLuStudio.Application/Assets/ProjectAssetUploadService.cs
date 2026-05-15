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
