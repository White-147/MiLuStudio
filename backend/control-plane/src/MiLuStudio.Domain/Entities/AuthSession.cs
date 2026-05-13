namespace MiLuStudio.Domain.Entities;

public sealed class AuthSession
{
    public required string Id { get; init; }

    public required string AccountId { get; set; }

    public required string DeviceId { get; set; }

    public required string AccessTokenHash { get; set; }

    public required string RefreshTokenHash { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset LastSeenAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
