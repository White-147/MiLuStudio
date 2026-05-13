namespace MiLuStudio.Domain.Entities;

using MiLuStudio.Domain;

public sealed class Account
{
    public required string Id { get; init; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public required string DisplayName { get; set; }

    public required string PasswordHash { get; set; }

    public AccountStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? LastLoginAt { get; set; }
}
