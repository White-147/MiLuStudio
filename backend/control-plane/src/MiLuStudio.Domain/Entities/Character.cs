namespace MiLuStudio.Domain.Entities;

public sealed class Character
{
    public required string Id { get; init; }

    public required string ProjectId { get; init; }

    public required string Name { get; set; }

    public required string RoleType { get; set; }

    public string? Gender { get; set; }

    public string? AgeRange { get; set; }

    public string? Personality { get; set; }

    public string? Appearance { get; set; }

    public string? Costume { get; set; }

    public string? VoiceProfile { get; set; }

    public string? ConsistencyNotes { get; set; }
}
