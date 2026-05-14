namespace MiLuStudio.Infrastructure.Assets;

using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Infrastructure.Configuration;
using Sha256Algorithm = global::System.Security.Cryptography.SHA256;

public sealed class LocalProjectAssetFileStore : IProjectAssetFileStore
{
    private readonly string _uploadsRoot;

    public LocalProjectAssetFileStore(IOptions<ControlPlaneOptions> options)
    {
        _uploadsRoot = EnsureDirectoryRoot(options.Value.UploadsRoot);
    }

    public async Task<StoredProjectAssetFile> SaveAsync(
        ProjectAssetFileSaveRequest request,
        CancellationToken cancellationToken)
    {
        var projectDirectory = ResolveInsideRoot(_uploadsRoot, SanitizePathPart(request.ProjectId));
        var assetDirectory = ResolveInsideRoot(projectDirectory, SanitizePathPart(request.AssetId));
        Directory.CreateDirectory(assetDirectory);

        var extension = Path.GetExtension(request.OriginalFileName).TrimStart('.').ToLowerInvariant();
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? "bin" : SanitizePathPart(extension);
        var localPath = ResolveInsideRoot(assetDirectory, $"original.{safeExtension}");

        await using var output = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.Read, 1024 * 1024, useAsync: true);
        using var sha256 = Sha256Algorithm.Create();

        var buffer = new byte[1024 * 1024];
        long bytesWritten = 0;

        while (true)
        {
            var read = await request.Content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
            {
                break;
            }

            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            sha256.TransformBlock(buffer, 0, read, null, 0);
            bytesWritten += read;
        }

        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        return new StoredProjectAssetFile(
            request.ProjectId,
            request.AssetId,
            request.OriginalFileName,
            localPath,
            safeExtension,
            request.ContentType,
            bytesWritten,
            Convert.ToHexString(sha256.Hash ?? Array.Empty<byte>()).ToLowerInvariant());
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
            throw new InvalidOperationException("Resolved upload path escaped the configured uploads root.");
        }

        return candidate;
    }

    private static string SanitizePathPart(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(character => invalid.Contains(character) ? '_' : character).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "asset" : sanitized[..Math.Min(sanitized.Length, 80)];
    }
}
