namespace MiLuStudio.Application.Assets;

public sealed record ProjectAssetUploadSessionCreateRequest(
    string? Intent,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    long? ChunkSize);

public sealed record ProjectAssetUploadSessionResponse(
    string Id,
    string ProjectId,
    string Kind,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    string? Intent,
    long ChunkSize,
    int TotalChunks,
    IReadOnlyList<int> UploadedChunks,
    string Status,
    string? AssetId,
    string CreatedAt,
    string ExpiresAt);

public sealed record ProjectAssetChunkUploadResponse(
    string SessionId,
    int ChunkIndex,
    long BytesReceived,
    string Sha256,
    IReadOnlyList<int> UploadedChunks,
    bool ReadyToComplete);

public sealed record ProjectAssetUploadCompleteResponse(
    ProjectAssetUploadResponse Asset,
    ProjectAssetUploadSessionResponse Session);
