namespace MiLuStudio.Application.Assets;

using JsonNode = global::System.Text.Json.Nodes.JsonNode;

public sealed record ProjectAssetAnalysisResponse(
    string Id,
    string ProjectId,
    string Kind,
    string MimeType,
    long FileSize,
    string? Sha256,
    string CreatedAt,
    string? OriginalFileName,
    string? Stage,
    string? AnalysisSchemaVersion,
    ProjectAssetAnalysisBoundary Boundary,
    ProjectAssetChunkManifestSummary ChunkManifestSummary,
    JsonNode? Parse,
    JsonNode? Upload,
    JsonNode? Parser,
    ProjectAssetOcrSummary Ocr,
    JsonNode? Text,
    JsonNode? ContentBlocks,
    JsonNode? ChunkManifest,
    JsonNode? DocumentStructure,
    JsonNode? Limits,
    ProjectAssetDerivativeSummary Derivatives,
    bool MetadataJsonParsed,
    string? MetadataParseError);

public sealed record ProjectAssetAnalysisBoundary(
    bool? UiElectronFileAccess,
    bool? GenerationPayloadSent,
    bool? ModelProviderUsed,
    bool? BackendAdapterOnly);

public sealed record ProjectAssetChunkManifestSummary(
    string Status,
    string Strategy,
    int TotalChunks,
    int ChunkSizeCharacters,
    int OverlapCharacters,
    bool UsableAsStoryCandidate);

public sealed record ProjectAssetDerivativeSummary(
    int Count,
    IReadOnlyList<string> Kinds,
    string AccessPolicy);

public sealed record ProjectAssetOcrSummary(
    string? Engine,
    string Status,
    bool Candidate,
    bool RuntimeAvailable,
    bool Invoked,
    int CheckedPathCount,
    string? Language,
    int ExtractedTextLength,
    bool? UiElectronFileAccess,
    bool? ModelProviderUsed);
