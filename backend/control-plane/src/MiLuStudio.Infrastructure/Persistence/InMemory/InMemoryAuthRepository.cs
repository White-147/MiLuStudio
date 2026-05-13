namespace MiLuStudio.Infrastructure.Persistence.InMemory;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class InMemoryAuthRepository : IAuthRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<string, Account> _accounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DeviceBinding> _devices = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AuthSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, LicenseGrant> _licenses = new(StringComparer.OrdinalIgnoreCase);

    public Task<Account?> FindAccountByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var account = _accounts.Values.FirstOrDefault(account =>
                string.Equals(account.Email, normalizedIdentifier, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(account.Phone, normalizedIdentifier, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(account.Id, normalizedIdentifier, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(account is null ? null : Clone(account));
        }
    }

    public Task<Account?> GetAccountAsync(string accountId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_accounts.TryGetValue(accountId, out var account) ? Clone(account) : null);
        }
    }

    public Task AddAccountAsync(Account account, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _accounts[account.Id] = Clone(account);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAccountAsync(Account account, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _accounts[account.Id] = Clone(account);
        }

        return Task.CompletedTask;
    }

    public Task<DeviceBinding?> FindDeviceAsync(string accountId, string machineFingerprintHash, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var device = _devices.Values.FirstOrDefault(device =>
                string.Equals(device.AccountId, accountId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(device.MachineFingerprintHash, machineFingerprintHash, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(device is null ? null : Clone(device));
        }
    }

    public Task<DeviceBinding?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_devices.TryGetValue(deviceId, out var device) ? Clone(device) : null);
        }
    }

    public Task<int> CountTrustedDevicesAsync(string accountId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_devices.Values.Count(device =>
                string.Equals(device.AccountId, accountId, StringComparison.OrdinalIgnoreCase) &&
                device.Trusted));
        }
    }

    public Task AddDeviceAsync(DeviceBinding device, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _devices[device.Id] = Clone(device);
        }

        return Task.CompletedTask;
    }

    public Task UpdateDeviceAsync(DeviceBinding device, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _devices[device.Id] = Clone(device);
        }

        return Task.CompletedTask;
    }

    public Task<AuthSession?> FindSessionByAccessTokenHashAsync(string accessTokenHash, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var session = _sessions.Values.FirstOrDefault(session => session.AccessTokenHash == accessTokenHash);
            return Task.FromResult(session is null ? null : Clone(session));
        }
    }

    public Task<AuthSession?> FindSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var session = _sessions.Values.FirstOrDefault(session => session.RefreshTokenHash == refreshTokenHash);
            return Task.FromResult(session is null ? null : Clone(session));
        }
    }

    public Task AddSessionAsync(AuthSession session, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _sessions[session.Id] = Clone(session);
        }

        return Task.CompletedTask;
    }

    public Task UpdateSessionAsync(AuthSession session, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _sessions[session.Id] = Clone(session);
        }

        return Task.CompletedTask;
    }

    public Task<LicenseGrant?> GetLatestLicenseAsync(string accountId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var license = _licenses.Values
                .Where(license => string.Equals(license.AccountId, accountId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(license => license.UpdatedAt)
                .FirstOrDefault();
            return Task.FromResult(license is null ? null : Clone(license));
        }
    }

    public Task<LicenseGrant?> GetActiveLicenseAsync(string accountId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var license = _licenses.Values
                .Where(license =>
                    string.Equals(license.AccountId, accountId, StringComparison.OrdinalIgnoreCase) &&
                    license.Status == LicenseStatus.Active &&
                    license.StartsAt <= now &&
                    license.ExpiresAt > now)
                .OrderByDescending(license => license.UpdatedAt)
                .FirstOrDefault();
            return Task.FromResult(license is null ? null : Clone(license));
        }
    }

    public Task AddLicenseAsync(LicenseGrant license, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _licenses[license.Id] = Clone(license);
        }

        return Task.CompletedTask;
    }

    public Task UpdateLicenseAsync(LicenseGrant license, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _licenses[license.Id] = Clone(license);
        }

        return Task.CompletedTask;
    }

    private static Account Clone(Account account)
    {
        return new Account
        {
            Id = account.Id,
            Email = account.Email,
            Phone = account.Phone,
            DisplayName = account.DisplayName,
            PasswordHash = account.PasswordHash,
            Status = account.Status,
            CreatedAt = account.CreatedAt,
            LastLoginAt = account.LastLoginAt
        };
    }

    private static DeviceBinding Clone(DeviceBinding device)
    {
        return new DeviceBinding
        {
            Id = device.Id,
            AccountId = device.AccountId,
            MachineFingerprintHash = device.MachineFingerprintHash,
            DeviceName = device.DeviceName,
            FirstSeenAt = device.FirstSeenAt,
            LastSeenAt = device.LastSeenAt,
            Trusted = device.Trusted
        };
    }

    private static AuthSession Clone(AuthSession session)
    {
        return new AuthSession
        {
            Id = session.Id,
            AccountId = session.AccountId,
            DeviceId = session.DeviceId,
            AccessTokenHash = session.AccessTokenHash,
            RefreshTokenHash = session.RefreshTokenHash,
            CreatedAt = session.CreatedAt,
            LastSeenAt = session.LastSeenAt,
            ExpiresAt = session.ExpiresAt,
            RevokedAt = session.RevokedAt
        };
    }

    private static LicenseGrant Clone(LicenseGrant license)
    {
        return new LicenseGrant
        {
            Id = license.Id,
            AccountId = license.AccountId,
            LicenseType = license.LicenseType,
            Plan = license.Plan,
            ActivationCodeHash = license.ActivationCodeHash,
            Status = license.Status,
            StartsAt = license.StartsAt,
            ExpiresAt = license.ExpiresAt,
            MaxDevices = license.MaxDevices,
            CreatedAt = license.CreatedAt,
            UpdatedAt = license.UpdatedAt
        };
    }
}
