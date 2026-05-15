namespace MiLuStudio.Infrastructure.Persistence.Sqlite;

using Microsoft.EntityFrameworkCore;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class SqliteAuthRepository : IAuthRepository
{
    private readonly MiLuStudioDbContext _db;

    public SqliteAuthRepository(MiLuStudioDbContext db)
    {
        _db = db;
        _db.Database.EnsureCreated();
    }

    public async Task<Account?> FindAccountByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken)
    {
        return await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account =>
                account.Email != null && account.Email.ToLower() == normalizedIdentifier ||
                account.Phone != null && account.Phone == normalizedIdentifier ||
                account.Id == normalizedIdentifier,
                cancellationToken);
    }

    public async Task<Account?> GetAccountAsync(string accountId, CancellationToken cancellationToken)
    {
        return await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(account => account.Id == accountId, cancellationToken);
    }

    public async Task AddAccountAsync(Account account, CancellationToken cancellationToken)
    {
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task UpdateAccountAsync(Account account, CancellationToken cancellationToken)
    {
        _db.Accounts.Update(account);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task<DeviceBinding?> FindDeviceAsync(
        string accountId,
        string machineFingerprintHash,
        CancellationToken cancellationToken)
    {
        return await _db.DeviceBindings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                device => device.AccountId == accountId && device.MachineFingerprintHash == machineFingerprintHash,
                cancellationToken);
    }

    public async Task<DeviceBinding?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken)
    {
        return await _db.DeviceBindings.AsNoTracking().FirstOrDefaultAsync(device => device.Id == deviceId, cancellationToken);
    }

    public async Task<int> CountTrustedDevicesAsync(string accountId, CancellationToken cancellationToken)
    {
        return await _db.DeviceBindings.CountAsync(
            device => device.AccountId == accountId && device.Trusted,
            cancellationToken);
    }

    public async Task AddDeviceAsync(DeviceBinding device, CancellationToken cancellationToken)
    {
        _db.DeviceBindings.Add(device);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task UpdateDeviceAsync(DeviceBinding device, CancellationToken cancellationToken)
    {
        _db.DeviceBindings.Update(device);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task<AuthSession?> FindSessionByAccessTokenHashAsync(
        string accessTokenHash,
        CancellationToken cancellationToken)
    {
        return await _db.AuthSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.AccessTokenHash == accessTokenHash, cancellationToken);
    }

    public async Task<AuthSession?> FindSessionByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken)
    {
        return await _db.AuthSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.RefreshTokenHash == refreshTokenHash, cancellationToken);
    }

    public async Task AddSessionAsync(AuthSession session, CancellationToken cancellationToken)
    {
        _db.AuthSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task UpdateSessionAsync(AuthSession session, CancellationToken cancellationToken)
    {
        _db.AuthSessions.Update(session);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task<LicenseGrant?> GetLatestLicenseAsync(string accountId, CancellationToken cancellationToken)
    {
        return await _db.LicenseGrants
            .AsNoTracking()
            .Where(license => license.AccountId == accountId)
            .OrderByDescending(license => license.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LicenseGrant?> GetActiveLicenseAsync(
        string accountId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        return await _db.LicenseGrants
            .AsNoTracking()
            .Where(license =>
                license.AccountId == accountId &&
                license.Status == LicenseStatus.Active &&
                license.StartsAt <= now &&
                license.ExpiresAt > now)
            .OrderByDescending(license => license.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddLicenseAsync(LicenseGrant license, CancellationToken cancellationToken)
    {
        _db.LicenseGrants.Add(license);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task UpdateLicenseAsync(LicenseGrant license, CancellationToken cancellationToken)
    {
        _db.LicenseGrants.Update(license);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }
}
