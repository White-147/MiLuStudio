namespace MiLuStudio.Application.Assets;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain.Entities;
using JsonArray = global::System.Text.Json.Nodes.JsonArray;
using JsonException = global::System.Text.Json.JsonException;
using JsonNode = global::System.Text.Json.Nodes.JsonNode;
using JsonObject = global::System.Text.Json.Nodes.JsonObject;
using JsonValue = global::System.Text.Json.Nodes.JsonValue;

public sealed class ProjectAssetAnalysisService
{
    private const string BackendAdapterOnlyAccessPolicy = "backend_adapter_only";

    private readonly IAssetRepository _assets;

    public ProjectAssetAnalysisService(IAssetRepository assets)
    {
        _assets = assets;
    }

    public async Task<ProjectAssetAnalysisResponse?> GetAsync(
        string projectId,
        string assetId,
        CancellationToken cancellationToken)
    {
        var assets = await _assets.ListAssetsByProjectAsync(projectId, cancellationToken);
        var asset = assets.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, assetId, StringComparison.OrdinalIgnoreCase));

        return asset is null ? null : BuildResponse(asset);
    }

    private static ProjectAssetAnalysisResponse BuildResponse(Asset asset)
    {
        var metadata = ParseMetadata(asset.MetadataJson, out var parseError);
        var technical = GetObject(metadata, "technical");
        var chunkManifest = GetObject(technical, "chunkManifest");
        var contentBlocks = GetNode(technical, "contentBlocks");
        var compressionPolicy = GetObject(technical, "compressionPolicy");
        var ocr = GetObject(technical, "ocr");

        return new ProjectAssetAnalysisResponse(
            asset.Id,
            asset.ProjectId,
            asset.Kind,
            asset.MimeType,
            asset.FileSize,
            asset.Sha256,
            asset.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            GetString(metadata, "originalFileName"),
            GetString(metadata, "stage"),
            GetString(metadata, "analysisSchemaVersion"),
            new ProjectAssetAnalysisBoundary(
                GetBool(GetObject(metadata, "upload"), "uiElectronFileAccess") ??
                    GetBool(compressionPolicy, "uiElectronFileAccess") ??
                    GetBool(GetObject(technical, "ocr"), "uiElectronFileAccess"),
                GetBool(GetObject(metadata, "parse"), "generationPayloadSent") ??
                    GetBool(technical, "generationPayloadSent"),
                GetBool(GetObject(metadata, "parse"), "modelProviderUsed") ??
                    GetBool(technical, "modelProviderUsed") ??
                    GetBool(ocr, "modelProviderUsed"),
                GetBool(compressionPolicy, "backendAdapterOnly")),
            BuildChunkManifestSummary(chunkManifest, contentBlocks),
            CloneNode(GetNode(metadata, "parse")),
            CloneNode(GetNode(metadata, "upload")),
            CloneNode(GetNode(technical, "parser")),
            BuildOcrSummary(ocr),
            CloneNode(GetNode(technical, "text")),
            CloneNode(contentBlocks),
            CloneNode(chunkManifest),
            CloneNode(GetNode(technical, "documentStructure")),
            CloneNode(GetNode(metadata, "limits")),
            BuildDerivativeSummary(metadata, technical),
            metadata is not null,
            parseError);
    }

    private static JsonObject? ParseMetadata(string? metadataJson, out string? parseError)
    {
        parseError = null;
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(metadataJson) as JsonObject;
        }
        catch (JsonException error)
        {
            parseError = error.Message;
            return null;
        }
    }

    private static ProjectAssetChunkManifestSummary BuildChunkManifestSummary(
        JsonObject? chunkManifest,
        JsonNode? contentBlocks)
    {
        return new ProjectAssetChunkManifestSummary(
            GetString(chunkManifest, "status") ?? "unavailable",
            GetString(chunkManifest, "strategy") ?? "unknown",
            GetInt(chunkManifest, "totalChunks") ?? 0,
            GetInt(chunkManifest, "chunkSizeCharacters") ?? 0,
            GetInt(chunkManifest, "overlapCharacters") ?? 0,
            HasUsableContentBlock(contentBlocks));
    }

    private static ProjectAssetOcrSummary BuildOcrSummary(JsonObject? ocr)
    {
        var checkedPathCount = 0;
        var runtimeAvailable = false;
        if (GetNode(ocr, "checkedPaths") is JsonArray checkedPaths)
        {
            checkedPathCount = checkedPaths.Count;
            runtimeAvailable = checkedPaths
                .OfType<JsonObject>()
                .Any(candidate => GetBool(candidate, "exists") == true);
        }

        return new ProjectAssetOcrSummary(
            GetString(ocr, "engine"),
            GetString(ocr, "status") ?? "not_recorded",
            GetBool(ocr, "candidate") == true,
            runtimeAvailable,
            GetBool(ocr, "invoked") == true,
            checkedPathCount,
            GetString(ocr, "language"),
            GetInt(ocr, "extractedTextLength") ?? 0,
            GetBool(ocr, "uiElectronFileAccess"),
            GetBool(ocr, "modelProviderUsed"));
    }

    private static ProjectAssetDerivativeSummary BuildDerivativeSummary(JsonObject? metadata, JsonObject? technical)
    {
        var kinds = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var count = 0;

        if (GetNode(metadata, "derivatives") is JsonArray derivatives)
        {
            count = derivatives.Count;
        }

        if (GetNode(technical, "derivativeDetails") is JsonArray details)
        {
            count = Math.Max(count, details.Count);
            foreach (var detail in details.OfType<JsonObject>())
            {
                var kind = GetString(detail, "kind");
                if (!string.IsNullOrWhiteSpace(kind))
                {
                    kinds.Add(kind);
                }
            }
        }

        return new ProjectAssetDerivativeSummary(count, kinds.ToList(), BackendAdapterOnlyAccessPolicy);
    }

    private static bool HasUsableContentBlock(JsonNode? contentBlocks)
    {
        if (contentBlocks is not JsonArray blocks)
        {
            return false;
        }

        return blocks
            .OfType<JsonObject>()
            .Any(block => GetBool(block, "usableAsStoryCandidate") == true);
    }

    private static JsonNode? GetNode(JsonObject? source, string property)
    {
        if (source is null)
        {
            return null;
        }

        return source.TryGetPropertyValue(property, out var value) ? value : null;
    }

    private static JsonObject? GetObject(JsonObject? source, string property)
    {
        return GetNode(source, property) as JsonObject;
    }

    private static JsonNode? CloneNode(JsonNode? node)
    {
        return node?.DeepClone();
    }

    private static string? GetString(JsonObject? source, string property)
    {
        if (GetNode(source, property) is not JsonValue value)
        {
            return null;
        }

        return value.TryGetValue<string>(out var result) ? result : null;
    }

    private static int? GetInt(JsonObject? source, string property)
    {
        if (GetNode(source, property) is not JsonValue value)
        {
            return null;
        }

        return value.TryGetValue<int>(out var result) ? result : null;
    }

    private static bool? GetBool(JsonObject? source, string property)
    {
        if (GetNode(source, property) is not JsonValue value)
        {
            return null;
        }

        return value.TryGetValue<bool>(out var result) ? result : null;
    }
}
