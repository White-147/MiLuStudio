namespace MiLuStudio.Application.Production;

public sealed record PersistSkillEnvelopeRequest(
    string OutputJson,
    string? AssetKind,
    string? Provider,
    string? Model,
    string? Unit,
    decimal? Quantity,
    decimal? EstimatedCost,
    decimal? ActualCost,
    bool? RequiresReview);

public sealed record PersistSkillEnvelopeResponse(
    string TaskId,
    string ProjectId,
    string JobId,
    string SkillName,
    string Status,
    string AssetId,
    string? CostLedgerEntryId);
