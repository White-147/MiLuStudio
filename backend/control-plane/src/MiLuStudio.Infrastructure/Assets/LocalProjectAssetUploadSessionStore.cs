namespace MiLuStudio.Infrastructure.Assets;

using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Assets;
using MiLuStudio.Infrastructure.Configuration;
using JsonSerializer = global::System.Text.Json.JsonSerializer;
using JsonSerializerOptions = global::System.Text.Json.JsonSerializerOptions;
using Sha256Algorithm = global::System.Security.Cryptography.SHA256;

public sealed class LocalProjectAssetUploadSessionStore : IProjectAssetUploadSessionStore
{
    private const string ActiveStatus = "active";
    private const string CompletedStatus = "completed";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _sessionRoot;

    public LocalProjectAssetUploadSessionStore(IOptions<ControlPlaneOptions> options)
    {
        var uploadsRoot = EnsureDirectoryRoot(options.Value.UploadsRoot);
        _sessionRoot = ResolveInsideRoot(uploadsRoot, ".upload-sessions");
        Directory.CreateDirectory(_sessionRoot);
    }

    public async Task<StoredProjectAssetUploadSession> CreateAsync(
        ProjectAssetUploadSessionCreateSpec request,
        CancellationToken cancellationToken)
    {
        var directory = ResolveSessionDirectory(request.ProjectId, request.SessionId);
        Directory.CreateDirectory(ResolveInsideRoot(directory, "chunks"));

        var manifest = new UploadSessionManifest
        {
            ProjectId = request.ProjectId,
            SessionId = request.SessionId,
            OriginalFileName = request.OriginalFileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            Intent = request.Intent,
            Kind = request.Kind,
            ChunkSize = request.ChunkSize,
            TotalChunks = request.TotalChunks,
            UploadedChunks = [],
            Status = ActiveStatus,
            CompletedAssetId = null,
            CreatedAt = request.CreatedAt,
            ExpiresAt = request.ExpiresAt
        };

        await WriteManifestAsync(directory, manifest, cancellationToken);
        return ToStored(manifest);
    }

    public async Task<StoredProjectAssetUploadSession?> GetAsync(
        string projectId,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var directory = ResolveSessionDirectory(projectId, sessionId);
        var manifestPath = ResolveInsideRoot(directory, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        var manifest = await ReadManifestAsync(manifestPath, cancellationToken);
        return manifest is null ? null : ToStored(manifest);
    }

    public async Task<ProjectAssetChunkSaveResult> SaveChunkAsync(
        ProjectAssetChunkSaveRequest request,
        CancellationToken cancellationToken)
    {
        var directory = ResolveSessionDirectory(request.Session.ProjectId, request.Session.SessionId);
        var manifestPath = ResolveInsideRoot(directory, "manifest.json");
        var manifest = await ReadManifestAsync(manifestPath, cancellationToken)
            ?? throw new ProjectAssetUploadException("Chunked upload session manifest was not found.");

        if (!string.Equals(manifest.Status, ActiveStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProjectAssetUploadException($"Chunked upload session is {manifest.Status}, not {ActiveStatus}.");
        }

        var chunksDirectory = ResolveInsideRoot(directory, "chunks");
        Directory.CreateDirectory(chunksDirectory);
        var chunkPath = ResolveInsideRoot(chunksDirectory, $"{request.ChunkIndex:D6}.part");
        var tempPath = ResolveInsideRoot(chunksDirectory, $"{request.ChunkIndex:D6}.{Guid.NewGuid():N}.tmp");

        string sha256;
        long bytesWritten = 0;
        await using (var output = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: true))
        using (var hash = Sha256Algorithm.Create())
        {
            var buffer = new byte[1024 * 1024];
            while (true)
            {
                var read = await request.Content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0)
                {
                    break;
                }

                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                hash.TransformBlock(buffer, 0, read, null, 0);
                bytesWritten += read;
            }

            hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            sha256 = Convert.ToHexString(hash.Hash ?? Array.Empty<byte>()).ToLowerInvariant();
        }

        if (bytesWritten != request.ExpectedByteCount)
        {
            File.Delete(tempPath);
            throw new ProjectAssetUploadException(
                $"Chunk {request.ChunkIndex} has {bytesWritten} bytes, expected {request.ExpectedByteCount} bytes.");
        }

        if (!string.IsNullOrWhiteSpace(request.ExpectedSha256) &&
            !string.Equals(sha256, request.ExpectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(tempPath);
            throw new ProjectAssetUploadException($"Chunk {request.ChunkIndex} SHA256 did not match the request header.");
        }

        File.Move(tempPath, chunkPath, overwrite: true);
        if (!manifest.UploadedChunks.Contains(request.ChunkIndex))
        {
            manifest.UploadedChunks.Add(request.ChunkIndex);
            manifest.UploadedChunks.Sort();
        }

        await WriteManifestAsync(directory, manifest, cancellationToken);
        var session = ToStored(manifest);
        return new ProjectAssetChunkSaveResult(session, request.ChunkIndex, bytesWritten, sha256);
    }

    public async Task<ProjectAssetUploadSessionAssembledFile> AssembleAsync(
        StoredProjectAssetUploadSession session,
        CancellationToken cancellationToken)
    {
        var directory = ResolveSessionDirectory(session.ProjectId, session.SessionId);
        var chunksDirectory = ResolveInsideRoot(directory, "chunks");
        var assembledDirectory = ResolveInsideRoot(directory, "assembled");
        Directory.CreateDirectory(assembledDirectory);

        var extension = Path.GetExtension(session.OriginalFileName).TrimStart('.').ToLowerInvariant();
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? "bin" : SanitizePathPart(extension);
        var assembledPath = ResolveInsideRoot(assembledDirectory, $"original.{safeExtension}");

        await using var output = new FileStream(assembledPath, FileMode.Create, FileAccess.Write, FileShare.Read, 1024 * 1024, useAsync: true);
        for (var index = 0; index < session.TotalChunks; index++)
        {
            var chunkPath = ResolveInsideRoot(chunksDirectory, $"{index:D6}.part");
            if (!File.Exists(chunkPath))
            {
                throw new ProjectAssetUploadException($"Chunk {index} is missing and the session cannot be completed.");
            }

            await using var input = new FileStream(chunkPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, useAsync: true);
            await input.CopyToAsync(output, cancellationToken);
        }

        return new ProjectAssetUploadSessionAssembledFile(session, assembledPath);
    }

    public async Task<StoredProjectAssetUploadSession> MarkCompletedAsync(
        StoredProjectAssetUploadSession session,
        string assetId,
        CancellationToken cancellationToken)
    {
        var directory = ResolveSessionDirectory(session.ProjectId, session.SessionId);
        var manifestPath = ResolveInsideRoot(directory, "manifest.json");
        var manifest = await ReadManifestAsync(manifestPath, cancellationToken)
            ?? throw new ProjectAssetUploadException("Chunked upload session manifest was not found.");

        manifest.Status = CompletedStatus;
        manifest.CompletedAssetId = assetId;
        manifest.UploadedChunks = Enumerable.Range(0, manifest.TotalChunks).ToList();
        await WriteManifestAsync(directory, manifest, cancellationToken);

        DeleteDirectoryIfExists(ResolveInsideRoot(directory, "chunks"));
        DeleteDirectoryIfExists(ResolveInsideRoot(directory, "assembled"));

        return ToStored(manifest);
    }

    private string ResolveSessionDirectory(string projectId, string sessionId)
    {
        var projectDirectory = ResolveInsideRoot(_sessionRoot, SanitizePathPart(projectId));
        return ResolveInsideRoot(projectDirectory, SanitizePathPart(sessionId));
    }

    private static async Task<UploadSessionManifest?> ReadManifestAsync(string manifestPath, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, useAsync: true);
        return await JsonSerializer.DeserializeAsync<UploadSessionManifest>(input, cancellationToken: cancellationToken);
    }

    private static async Task WriteManifestAsync(
        string directory,
        UploadSessionManifest manifest,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(directory);
        var manifestPath = ResolveInsideRoot(directory, "manifest.json");
        var tempPath = ResolveInsideRoot(directory, $"manifest.{Guid.NewGuid():N}.tmp");
        await using (var output = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
        {
            await JsonSerializer.SerializeAsync(output, manifest, JsonOptions, cancellationToken);
        }

        File.Move(tempPath, manifestPath, overwrite: true);
    }

    private static StoredProjectAssetUploadSession ToStored(UploadSessionManifest manifest)
    {
        return new StoredProjectAssetUploadSession(
            manifest.ProjectId,
            manifest.SessionId,
            manifest.OriginalFileName,
            manifest.ContentType,
            manifest.FileSize,
            manifest.Intent,
            manifest.Kind,
            manifest.ChunkSize,
            manifest.TotalChunks,
            manifest.UploadedChunks.OrderBy(chunk => chunk).ToList(),
            string.IsNullOrWhiteSpace(manifest.Status) ? ActiveStatus : manifest.Status,
            manifest.CompletedAssetId,
            manifest.CreatedAt,
            manifest.ExpiresAt);
    }

    private static string EnsureDirectoryRoot(string path)
    {
        var root = Path.GetFullPath(string.IsNullOrWhiteSpace(path)
            ? "D:\\code\\MiLuStudio\\uploads"
            : path);
        Directory.CreateDirectory(root);
        return root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string ResolveInsideRoot(string root, string child)
    {
        var candidate = Path.GetFullPath(Path.Combine(root, child));
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (!candidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(candidate, normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Resolved upload session path escaped the configured uploads root.");
        }

        return candidate;
    }

    private static string SanitizePathPart(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(character => invalid.Contains(character) ? '_' : character).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "upload" : sanitized[..Math.Min(sanitized.Length, 80)];
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private sealed class UploadSessionManifest
    {
        public string ProjectId { get; set; } = string.Empty;

        public string SessionId { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = "application/octet-stream";

        public long FileSize { get; set; }

        public string? Intent { get; set; }

        public string Kind { get; set; } = "reference";

        public long ChunkSize { get; set; }

        public int TotalChunks { get; set; }

        public List<int> UploadedChunks { get; set; } = [];

        public string Status { get; set; } = ActiveStatus;

        public string? CompletedAssetId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }
    }
}
