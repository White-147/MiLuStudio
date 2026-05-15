namespace MiLuStudio.Application.Assets;

using MiLuStudio.Application.Abstractions;

public sealed class ProjectAssetChunkUploadService
{
    private const string ActiveStatus = "active";
    private const int SessionTtlHours = 24;

    private readonly IClock _clock;
    private readonly IProjectRepository _projects;
    private readonly IProjectAssetUploadSessionStore _sessions;
    private readonly ProjectAssetUploadService _uploads;

    public ProjectAssetChunkUploadService(
        IProjectRepository projects,
        IProjectAssetUploadSessionStore sessions,
        ProjectAssetUploadService uploads,
        IClock clock)
    {
        _projects = projects;
        _sessions = sessions;
        _uploads = uploads;
        _clock = clock;
    }

    public async Task<ProjectAssetUploadSessionResponse?> CreateAsync(
        string projectId,
        ProjectAssetUploadSessionCreateRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(projectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
        {
            throw new ProjectAssetUploadException("Original file name is required for chunked upload.");
        }

        var kind = ProjectAssetUploadService.ClassifyKind(
            request.OriginalFileName,
            NormalizeContentType(request.ContentType),
            request.Intent);
        ProjectAssetUploadService.ValidateSize(kind, request.FileSize, request.OriginalFileName);

        var chunkSize = NormalizeChunkSize(request.ChunkSize);
        var totalChunks = checked((int)Math.Ceiling(request.FileSize / (double)chunkSize));
        var now = _clock.Now;
        var session = await _sessions.CreateAsync(
            new ProjectAssetUploadSessionCreateSpec(
                projectId,
                CreateId("upload_session"),
                request.OriginalFileName.Trim(),
                NormalizeContentType(request.ContentType),
                request.FileSize,
                request.Intent,
                kind,
                chunkSize,
                totalChunks,
                now,
                now.AddHours(SessionTtlHours)),
            cancellationToken);

        return ToResponse(session);
    }

    public async Task<ProjectAssetUploadSessionResponse?> GetAsync(
        string projectId,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var session = await _sessions.GetAsync(projectId, sessionId, cancellationToken);
        return session is null ? null : ToResponse(session);
    }

    public async Task<ProjectAssetChunkUploadResponse?> UploadChunkAsync(
        string projectId,
        string sessionId,
        int chunkIndex,
        Stream content,
        string? expectedSha256,
        CancellationToken cancellationToken)
    {
        var session = await _sessions.GetAsync(projectId, sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        EnsureActive(session);
        if (chunkIndex < 0 || chunkIndex >= session.TotalChunks)
        {
            throw new ProjectAssetUploadException($"Chunk index {chunkIndex} is outside the session range 0..{session.TotalChunks - 1}.");
        }

        var expectedByteCount = ExpectedChunkByteCount(session, chunkIndex);
        var saved = await _sessions.SaveChunkAsync(
            new ProjectAssetChunkSaveRequest(
                session,
                chunkIndex,
                expectedByteCount,
                content,
                NormalizeSha256(expectedSha256)),
            cancellationToken);

        return new ProjectAssetChunkUploadResponse(
            saved.Session.SessionId,
            saved.ChunkIndex,
            saved.BytesReceived,
            saved.Sha256,
            saved.Session.UploadedChunks,
            saved.Session.UploadedChunks.Count == saved.Session.TotalChunks);
    }

    public async Task<ProjectAssetUploadCompleteResponse?> CompleteAsync(
        string projectId,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var session = await _sessions.GetAsync(projectId, sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        EnsureActive(session);
        if (session.UploadedChunks.Count != session.TotalChunks)
        {
            throw new ProjectAssetUploadException($"Chunked upload session is incomplete: {session.UploadedChunks.Count}/{session.TotalChunks} chunks uploaded.");
        }

        var assembled = await _sessions.AssembleAsync(session, cancellationToken);
        ProjectAssetUploadResponse? uploaded;
        await using (var stream = new FileStream(assembled.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, useAsync: true))
        {
            uploaded = await _uploads.UploadAsync(
                projectId,
                new ProjectAssetUploadRequest(
                    session.Intent,
                    session.OriginalFileName,
                    session.ContentType,
                    session.FileSize,
                    stream,
                    "control_api_resumable_chunks"),
                cancellationToken);
        }

        if (uploaded is null)
        {
            return null;
        }

        var completed = await _sessions.MarkCompletedAsync(session, uploaded.Id, cancellationToken);
        return new ProjectAssetUploadCompleteResponse(uploaded, ToResponse(completed));
    }

    private static void EnsureActive(StoredProjectAssetUploadSession session)
    {
        if (!string.Equals(session.Status, ActiveStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProjectAssetUploadException($"Chunked upload session is {session.Status}, not {ActiveStatus}.");
        }
    }

    private static long ExpectedChunkByteCount(StoredProjectAssetUploadSession session, int chunkIndex)
    {
        if (chunkIndex == session.TotalChunks - 1)
        {
            return session.FileSize - session.ChunkSize * chunkIndex;
        }

        return session.ChunkSize;
    }

    private static long NormalizeChunkSize(long? chunkSize)
    {
        var value = chunkSize ?? ProjectAssetUploadService.PreferredUploadChunkBytes;
        if (value < ProjectAssetUploadService.MinUploadChunkBytes || value > ProjectAssetUploadService.MaxUploadChunkBytes)
        {
            throw new ProjectAssetUploadException(
                $"Chunk size must be between {ProjectAssetUploadService.MinUploadChunkBytes} and {ProjectAssetUploadService.MaxUploadChunkBytes} bytes.");
        }

        return value;
    }

    private static string? NormalizeSha256(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    }

    private static string NormalizeContentType(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "application/octet-stream" : value.Trim();
    }

    private static ProjectAssetUploadSessionResponse ToResponse(StoredProjectAssetUploadSession session)
    {
        return new ProjectAssetUploadSessionResponse(
            session.SessionId,
            session.ProjectId,
            session.Kind,
            session.OriginalFileName,
            session.ContentType,
            session.FileSize,
            session.Intent,
            session.ChunkSize,
            session.TotalChunks,
            session.UploadedChunks,
            string.IsNullOrWhiteSpace(session.Status) ? ActiveStatus : session.Status,
            session.CompletedAssetId,
            session.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            session.ExpiresAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
    }

    private static string CreateId(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }
}
