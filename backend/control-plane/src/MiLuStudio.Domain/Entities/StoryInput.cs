namespace MiLuStudio.Domain.Entities;

public sealed class StoryInput
{
    public required string Id { get; init; }

    public required string ProjectId { get; init; }

    public required string SourceType { get; init; }

    public required string OriginalText { get; set; }

    public string? FileAssetId { get; set; }

    public required string Language { get; init; }

    public int WordCount { get; set; }

    public DateTimeOffset? ParsedAt { get; set; }
}
