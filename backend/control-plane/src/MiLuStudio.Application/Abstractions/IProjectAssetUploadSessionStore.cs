namespace MiLuStudio.Application.Abstractions;

public interface IProjectAssetUploadSessionStore
{
    Task<StoredProjectAssetUploadSession> CreateAsync(
        ProjectAssetUploadSessionCreateSpec request,
        CancellationToken cancellationToken);

    Task<StoredProjectAssetUploadSession?> GetAsync(
        string projectId,
        string sessionId,
        CancellationToken cancellationToken);

    Task<ProjectAssetChunkSaveResult> SaveChunkAsync(
        ProjectAssetChunkSaveRequest request,
        CancellationToken cancellationToken);

    Task<ProjectAssetUploadSessionAssembledFile> AssembleAsync(
        StoredProjectAssetUploadSession session,
        CancellationToken cancellationToken);

    Task<StoredProjectAssetUploadSession> MarkCompletedAsync(
        StoredProjectAssetUploadSession session,
        string assetId,
        CancellationToken cancellationToken);
}

public sealed record ProjectAssetUploadSessionCreateSpec(
    string ProjectId,
    string SessionId,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    string? Intent,
    string Kind,
    long ChunkSize,
    int TotalChunks,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public sealed record StoredProjectAssetUploadSession(
    string ProjectId,
    string SessionId,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    string? Intent,
    string Kind,
    long ChunkSize,
    int TotalChunks,
    IReadOnlyList<int> UploadedChunks,
    string Status,
    string? CompletedAssetId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public sealed record ProjectAssetChunkSaveRequest(
    StoredProjectAssetUploadSession Session,
    int ChunkIndex,
    long ExpectedByteCount,
    Stream Content,
    string? ExpectedSha256);

public sealed record ProjectAssetChunkSaveResult(
    StoredProjectAssetUploadSession Session,
    int ChunkIndex,
    long BytesReceived,
    string Sha256);

public sealed record ProjectAssetUploadSessionAssembledFile(
    StoredProjectAssetUploadSession Session,
    string LocalPath);
