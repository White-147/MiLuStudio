namespace MiLuStudio.Domain.Entities;

public sealed class CostLedgerEntry
{
    public required string Id { get; init; }

    public required string ProjectId { get; init; }

    public string? TaskId { get; init; }

    public required string Provider { get; init; }

    public required string Model { get; init; }

    public required string Unit { get; init; }

    public decimal Quantity { get; init; }

    public decimal EstimatedCost { get; init; }

    public decimal? ActualCost { get; set; }

    public DateTimeOffset CreatedAt { get; init; }
}
