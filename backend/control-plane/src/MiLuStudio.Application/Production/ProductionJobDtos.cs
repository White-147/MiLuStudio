namespace MiLuStudio.Application.Production;

public sealed record StartProductionJobRequest(string? RequestedBy);

public sealed record ProductionCheckpointRequest(bool? Approved, string? Notes);

public sealed record ProductionJobDto(
    string Id,
    string ProjectId,
    string Status,
    string CurrentStage,
    int Progress,
    string StartedAt,
    string? FinishedAt,
    string? ErrorMessage,
    IReadOnlyList<ProductionStageDto> Stages);

public sealed record ProductionStageDto(
    string Id,
    string Label,
    string Skill,
    string Status,
    string Duration,
    string Cost,
    bool NeedsReview);

public sealed record ProductionJobEventDto(
    string Type,
    string JobId,
    string ProjectId,
    string StageId,
    string StageLabel,
    string Skill,
    string Status,
    string JobStatus,
    int Progress,
    string Message,
    DateTimeOffset OccurredAt);
