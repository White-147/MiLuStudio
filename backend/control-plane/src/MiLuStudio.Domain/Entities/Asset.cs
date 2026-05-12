namespace MiLuStudio.Domain.Entities;

public sealed class Asset
{
    public required string Id { get; init; }

    public required string ProjectId { get; init; }

    public required string Kind { get; set; }

    public required string LocalPath { get; set; }

    public required string MimeType { get; set; }

    public long FileSize { get; set; }

    public string? Sha256 { get; set; }

    public string? MetadataJson { get; set; }

    public DateTimeOffset CreatedAt { get; init; }
}
