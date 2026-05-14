namespace MiLuStudio.Application.Abstractions;

public interface IAssetTechnicalAnalyzer
{
    Task<ProjectAssetTechnicalAnalysis> AnalyzeAsync(
        StoredProjectAssetFile file,
        string kind,
        CancellationToken cancellationToken);
}

public sealed record ProjectAssetTechnicalAnalysis(
    string Status,
    string Message,
    string? ExtractedText,
    IReadOnlyList<string> DerivativePaths,
    IReadOnlyDictionary<string, object?> Metadata);
