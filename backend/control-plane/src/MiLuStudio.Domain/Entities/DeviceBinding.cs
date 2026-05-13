namespace MiLuStudio.Domain.Entities;

public sealed class DeviceBinding
{
    public required string Id { get; init; }

    public required string AccountId { get; init; }

    public required string MachineFingerprintHash { get; init; }

    public required string DeviceName { get; set; }

    public DateTimeOffset FirstSeenAt { get; init; }

    public DateTimeOffset LastSeenAt { get; set; }

    public bool Trusted { get; set; }
}
