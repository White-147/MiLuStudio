namespace MiLuStudio.Application.Auth;

public sealed record RegisterAccountRequest(
    string? Email,
    string? Phone,
    string? DisplayName,
    string? Password,
    string? DeviceFingerprint,
    string? DeviceName,
    string? ActivationCode);

public sealed record LoginRequest(
    string? Identifier,
    string? Password,
    string? DeviceFingerprint,
    string? DeviceName);

public sealed record RefreshSessionRequest(
    string? RefreshToken,
    string? DeviceFingerprint,
    string? DeviceName);

public sealed record LogoutRequest(string? RefreshToken);

public sealed record ActivateLicenseRequest(
    string? ActivationCode,
    string? DeviceFingerprint,
    string? DeviceName);

public sealed record BindDeviceRequest(
    string? DeviceFingerprint,
    string? DeviceName);

public sealed record AuthAccountDto(
    string Id,
    string? Email,
    string? Phone,
    string DisplayName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt);

public sealed record AuthDeviceDto(
    string Id,
    string DeviceName,
    bool Trusted,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastSeenAt);

public sealed record LicenseStatusDto(
    string Status,
    bool IsActive,
    string Plan,
    string LicenseType,
    DateTimeOffset? StartsAt,
    DateTimeOffset? ExpiresAt,
    int MaxDevices,
    string Message);

public sealed record AuthSessionDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    AuthAccountDto Account,
    AuthDeviceDto Device,
    LicenseStatusDto License);

public sealed record AuthStateDto(
    bool Authenticated,
    AuthAccountDto? Account,
    AuthDeviceDto? Device,
    LicenseStatusDto License,
    string? ErrorCode,
    string Message);

public sealed record AuthValidationResult(
    bool Allowed,
    int StatusCode,
    string Code,
    string Message,
    AuthenticatedPrincipal? Principal,
    LicenseStatusDto? License);

public sealed record AuthenticatedPrincipal(AccountSnapshot Account, DeviceSnapshot Device, string SessionId);

public sealed record AccountSnapshot(string Id, string DisplayName, string? Email, string? Phone);

public sealed record DeviceSnapshot(string Id, string DeviceName);
