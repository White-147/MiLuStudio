namespace MiLuStudio.Domain.Entities;

using MiLuStudio.Domain;

public sealed class Project
{
    public required string Id { get; init; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public ProjectMode Mode { get; set; }

    public ProjectStatus Status { get; set; }

    public int TargetDurationSeconds { get; set; }

    public required string AspectRatio { get; set; }

    public required string StylePreset { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; set; }
}
