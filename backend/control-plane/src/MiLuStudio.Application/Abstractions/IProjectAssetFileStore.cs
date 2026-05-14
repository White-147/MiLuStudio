namespace MiLuStudio.Application.Abstractions;

public interface IProjectAssetFileStore
{
    Task<StoredProjectAssetFile> SaveAsync(
        ProjectAssetFileSaveRequest request,
        CancellationToken cancellationToken);
}

public sealed record ProjectAssetFileSaveRequest(
    string ProjectId,
    string AssetId,
    string OriginalFileName,
    string ContentType,
    long DeclaredFileSize,
    Stream Content);

public sealed record StoredProjectAssetFile(
    string ProjectId,
    string AssetId,
    string OriginalFileName,
    string LocalPath,
    string Extension,
    string ContentType,
    long FileSize,
    string Sha256);
