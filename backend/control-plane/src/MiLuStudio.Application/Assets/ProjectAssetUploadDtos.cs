namespace MiLuStudio.Application.Assets;

public sealed record ProjectAssetUploadRequest(
    string? Intent,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    Stream Content);

public sealed record ProjectAssetUploadResponse(
    string Id,
    string ProjectId,
    string Kind,
    string OriginalFileName,
    string LocalPath,
    string MimeType,
    long FileSize,
    string Sha256,
    string MetadataJson,
    string CreatedAt,
    string? ExtractedText,
    string Message);
