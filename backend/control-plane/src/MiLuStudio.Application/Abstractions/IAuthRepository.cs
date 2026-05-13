namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Domain.Entities;

public interface IAuthRepository
{
    Task<Account?> FindAccountByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken);

    Task<Account?> GetAccountAsync(string accountId, CancellationToken cancellationToken);

    Task AddAccountAsync(Account account, CancellationToken cancellationToken);

    Task UpdateAccountAsync(Account account, CancellationToken cancellationToken);

    Task<DeviceBinding?> FindDeviceAsync(string accountId, string machineFingerprintHash, CancellationToken cancellationToken);

    Task<DeviceBinding?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken);

    Task<int> CountTrustedDevicesAsync(string accountId, CancellationToken cancellationToken);

    Task AddDeviceAsync(DeviceBinding device, CancellationToken cancellationToken);

    Task UpdateDeviceAsync(DeviceBinding device, CancellationToken cancellationToken);

    Task<AuthSession?> FindSessionByAccessTokenHashAsync(string accessTokenHash, CancellationToken cancellationToken);

    Task<AuthSession?> FindSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken);

    Task AddSessionAsync(AuthSession session, CancellationToken cancellationToken);

    Task UpdateSessionAsync(AuthSession session, CancellationToken cancellationToken);

    Task<LicenseGrant?> GetLatestLicenseAsync(string accountId, CancellationToken cancellationToken);

    Task<LicenseGrant?> GetActiveLicenseAsync(string accountId, DateTimeOffset now, CancellationToken cancellationToken);

    Task AddLicenseAsync(LicenseGrant license, CancellationToken cancellationToken);

    Task UpdateLicenseAsync(LicenseGrant license, CancellationToken cancellationToken);
}
