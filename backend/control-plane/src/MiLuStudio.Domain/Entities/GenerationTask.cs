namespace MiLuStudio.Domain.Entities;

using MiLuStudio.Domain;

public sealed class GenerationTask
{
    public required string Id { get; init; }

    public required string JobId { get; init; }

    public required string ProjectId { get; init; }

    public string? ShotId { get; init; }

    public int QueueIndex { get; init; }

    public required string SkillName { get; init; }

    public required string Provider { get; init; }

    public required string InputJson { get; set; }

    public string? OutputJson { get; set; }

    public GenerationTaskStatus Status { get; set; }

    public int AttemptCount { get; set; }

    public decimal CostEstimate { get; init; }

    public decimal? CostActual { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public string? LockedBy { get; set; }

    public DateTimeOffset? LockedUntil { get; set; }

    public DateTimeOffset? LastHeartbeatAt { get; set; }

    public string? CheckpointNotes { get; set; }

    public string? ErrorMessage { get; set; }
}
