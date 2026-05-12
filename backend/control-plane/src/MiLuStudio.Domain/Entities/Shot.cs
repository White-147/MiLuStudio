namespace MiLuStudio.Domain.Entities;

public sealed class Shot
{
    public required string Id { get; init; }

    public required string ProjectId { get; init; }

    public string? EpisodeId { get; init; }

    public int ShotIndex { get; set; }

    public int DurationSeconds { get; set; }

    public required string SceneSummary { get; set; }

    public string? Dialogue { get; set; }

    public string? Narration { get; set; }

    public required string CharactersJson { get; set; }

    public string? CameraAngle { get; set; }

    public string? CameraMotion { get; set; }

    public string? Lighting { get; set; }

    public string? Composition { get; set; }

    public string? ImagePrompt { get; set; }

    public string? VideoPrompt { get; set; }

    public required string Status { get; set; }

    public bool UserLocked { get; set; }
}
