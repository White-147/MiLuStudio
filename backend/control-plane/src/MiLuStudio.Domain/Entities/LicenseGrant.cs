namespace MiLuStudio.Domain.Entities;

using MiLuStudio.Domain;

public sealed class LicenseGrant
{
    public required string Id { get; init; }

    public required string AccountId { get; init; }

    public LicenseKind LicenseType { get; set; }

    public required string Plan { get; set; }

    public string? ActivationCodeHash { get; set; }

    public LicenseStatus Status { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public int MaxDevices { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; set; }
}
