namespace MiLuStudio.Domain.Entities;

using MiLuStudio.Domain;

public sealed class ProductionJob
{
    public required string Id { get; init; }

    public required string ProjectId { get; init; }

    public required string CurrentStage { get; set; }

    public ProductionJobStatus Status { get; set; }

    public int ProgressPercent { get; set; }

    public DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
