namespace MiLuStudio.Application.Auth;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class AuthLicensingService
{
    private const int SessionLifetimeDays = 14;
    private const int MinimumPasswordLength = 8;
    private const string MissingLicensePlan = "unlicensed";

    private readonly IAuthLicensingAdapter _licensingAdapter;
    private readonly IAuthRepository _repository;
    private readonly IAuthTokenService _tokens;
    private readonly IClock _clock;
    private readonly IPasswordHasher _passwords;

    public AuthLicensingService(
        IAuthRepository repository,
        IAuthLicensingAdapter licensingAdapter,
        IAuthTokenService tokens,
        IClock clock,
        IPasswordHasher passwords)
    {
        _repository = repository;
        _licensingAdapter = licensingAdapter;
        _tokens = tokens;
        _clock = clock;
        _passwords = passwords;
    }

    public async Task<AuthSessionDto> RegisterAsync(RegisterAccountRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeOptional(request.Email)?.ToLowerInvariant();
        var phone = NormalizeOptional(request.Phone);
        var identifier = email ?? phone;

        if (identifier is null)
        {
            throw AuthCommandException.BadRequest("account_identifier_required", "请填写邮箱或手机号。");
        }

        if (await _repository.FindAccountByIdentifierAsync(identifier, cancellationToken) is not null)
        {
            throw AuthCommandException.Conflict("account_exists", "该账号已注册，请直接登录。");
        }

        var password = NormalizePassword(request.Password);
        var now = _clock.Now;
        var account = new Account
        {
            Id = CreateId("acct"),
            Email = email,
            Phone = phone,
            DisplayName = NormalizeOptional(request.DisplayName) ?? "MiLuStudio 用户",
            PasswordHash = _passwords.HashPassword(password),
            Status = AccountStatus.Active,
            CreatedAt = now,
            LastLoginAt = now
        };

        await _repository.AddAccountAsync(account, cancellationToken);
        var device = await BindDeviceInternalAsync(account, request.DeviceFingerprint, request.DeviceName, null, now, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.ActivationCode))
        {
            await ActivateInternalAsync(account, device, request.ActivationCode, now, cancellationToken);
        }

        return await CreateSessionAsync(account, device, now, cancellationToken);
    }

    public async Task<AuthSessionDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var identifier = NormalizeOptional(request.Identifier)?.ToLowerInvariant();
        if (identifier is null)
        {
            throw AuthCommandException.BadRequest("account_identifier_required", "请填写邮箱、手机号或账号标识。");
        }

        var account = await _repository.FindAccountByIdentifierAsync(identifier, cancellationToken)
            ?? throw AuthCommandException.Unauthorized("invalid_credentials", "账号或密码不正确。");

        if (account.Status != AccountStatus.Active)
        {
            throw AuthCommandException.Forbidden("account_unavailable", "账号当前不可用。");
        }

        var password = NormalizeOptional(request.Password) ?? string.Empty;
        if (!_passwords.VerifyPassword(password, account.PasswordHash))
        {
            throw AuthCommandException.Unauthorized("invalid_credentials", "账号或密码不正确。");
        }

        var now = _clock.Now;
        account.LastLoginAt = now;
        await _repository.UpdateAccountAsync(account, cancellationToken);
        var activeLicense = await _repository.GetActiveLicenseAsync(account.Id, now, cancellationToken);
        var device = await BindDeviceInternalAsync(
            account,
            request.DeviceFingerprint,
            request.DeviceName,
            activeLicense?.MaxDevices,
            now,
            cancellationToken);

        return await CreateSessionAsync(account, device, now, cancellationToken);
    }

    public async Task<AuthSessionDto> RefreshAsync(RefreshSessionRequest request, CancellationToken cancellationToken)
    {
        var refreshToken = NormalizeOptional(request.RefreshToken)
            ?? throw AuthCommandException.Unauthorized("refresh_token_required", "刷新会话需要 refresh token。");
        var refreshTokenHash = _tokens.HashToken(refreshToken);
        var session = await _repository.FindSessionByRefreshTokenHashAsync(refreshTokenHash, cancellationToken)
            ?? throw AuthCommandException.Unauthorized("invalid_refresh_token", "会话已失效，请重新登录。");

        var now = _clock.Now;
        if (session.RevokedAt is not null || session.ExpiresAt <= now)
        {
            throw AuthCommandException.Unauthorized("session_expired", "会话已过期，请重新登录。");
        }

        var account = await _repository.GetAccountAsync(session.AccountId, cancellationToken)
            ?? throw AuthCommandException.Unauthorized("account_missing", "账号不存在，请重新登录。");
        var activeLicense = await _repository.GetActiveLicenseAsync(account.Id, now, cancellationToken);
        var device = await _repository.GetDeviceAsync(session.DeviceId, cancellationToken)
            ?? await BindDeviceInternalAsync(
                account,
                request.DeviceFingerprint,
                request.DeviceName,
                activeLicense?.MaxDevices,
                now,
                cancellationToken);

        return await RotateSessionAsync(session, account, device, now, cancellationToken);
    }

    public async Task LogoutAsync(string? accessToken, LogoutRequest request, CancellationToken cancellationToken)
    {
        var session = await FindSessionAsync(accessToken, cancellationToken);
        if (session is null && !string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            session = await _repository.FindSessionByRefreshTokenHashAsync(_tokens.HashToken(request.RefreshToken), cancellationToken);
        }

        if (session is null || session.RevokedAt is not null)
        {
            return;
        }

        session.RevokedAt = _clock.Now;
        await _repository.UpdateSessionAsync(session, cancellationToken);
    }

    public async Task<AuthStateDto> GetStateAsync(string? accessToken, CancellationToken cancellationToken)
    {
        var principal = await ValidateAccessTokenInternalAsync(accessToken, cancellationToken);
        if (principal is null)
        {
            return new AuthStateDto(
                false,
                null,
                null,
                MissingLicense("请先登录或注册账号。"),
                "not_authenticated",
                "请先登录或注册账号。");
        }

        var license = await GetLicenseStatusAsync(principal.Account.Id, cancellationToken);
        return new AuthStateDto(
            true,
            ToAccountDto(principal.AccountEntity),
            ToDeviceDto(principal.DeviceEntity),
            license,
            license.IsActive ? null : "license_required",
            license.IsActive ? "账号已登录且许可证有效。" : license.Message);
    }

    public async Task<LicenseStatusDto> GetLicenseAsync(string? accessToken, CancellationToken cancellationToken)
    {
        var principal = await ValidateAccessTokenInternalAsync(accessToken, cancellationToken)
            ?? throw AuthCommandException.Unauthorized("not_authenticated", "请先登录或注册账号。");

        return await GetLicenseStatusAsync(principal.Account.Id, cancellationToken);
    }

    public async Task<AuthStateDto> ActivateAsync(
        string? accessToken,
        ActivateLicenseRequest request,
        CancellationToken cancellationToken)
    {
        var principal = await ValidateAccessTokenInternalAsync(accessToken, cancellationToken)
            ?? throw AuthCommandException.Unauthorized("not_authenticated", "请先登录或注册账号。");

        var now = _clock.Now;
        var device = principal.DeviceEntity;
        if (!string.IsNullOrWhiteSpace(request.DeviceFingerprint))
        {
            device = await BindDeviceInternalAsync(
                principal.AccountEntity,
                request.DeviceFingerprint,
                request.DeviceName ?? principal.DeviceEntity.DeviceName,
                null,
                now,
                cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.DeviceName) &&
                 !string.Equals(request.DeviceName.Trim(), device.DeviceName, StringComparison.Ordinal))
        {
            device.DeviceName = request.DeviceName.Trim();
            device.LastSeenAt = now;
            await _repository.UpdateDeviceAsync(device, cancellationToken);
        }

        await ActivateInternalAsync(principal.AccountEntity, device, request.ActivationCode, now, cancellationToken);
        return await GetStateAsync(accessToken, cancellationToken);
    }

    public async Task<AuthStateDto> BindDeviceAsync(
        string? accessToken,
        BindDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var principal = await ValidateAccessTokenInternalAsync(accessToken, cancellationToken)
            ?? throw AuthCommandException.Unauthorized("not_authenticated", "请先登录或注册账号。");

        var now = _clock.Now;
        var activeLicense = await _repository.GetActiveLicenseAsync(principal.Account.Id, now, cancellationToken);
        await BindDeviceInternalAsync(
            principal.AccountEntity,
            request.DeviceFingerprint,
            request.DeviceName,
            activeLicense?.MaxDevices,
            now,
            cancellationToken);
        return await GetStateAsync(accessToken, cancellationToken);
    }

    public async Task<AuthValidationResult> ValidateRequestAsync(
        string? accessToken,
        bool requireActiveLicense,
        CancellationToken cancellationToken)
    {
        var principal = await ValidateAccessTokenInternalAsync(accessToken, cancellationToken);
        if (principal is null)
        {
            return new AuthValidationResult(
                false,
                401,
                "not_authenticated",
                "请先登录或注册账号。",
                null,
                MissingLicense("请先登录或注册账号。"));
        }

        var license = await GetLicenseStatusAsync(principal.Account.Id, cancellationToken);
        if (requireActiveLicense && !license.IsActive)
        {
            return new AuthValidationResult(
                false,
                403,
                "license_required",
                license.Message,
                ToPrincipal(principal),
                license);
        }

        return new AuthValidationResult(true, 200, "ok", "authorized", ToPrincipal(principal), license);
    }

    private async Task<AuthSessionDto> CreateSessionAsync(
        Account account,
        DeviceBinding device,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var accessToken = _tokens.CreateToken();
        var refreshToken = _tokens.CreateToken();
        var session = new AuthSession
        {
            Id = CreateId("sess"),
            AccountId = account.Id,
            DeviceId = device.Id,
            AccessTokenHash = _tokens.HashToken(accessToken),
            RefreshTokenHash = _tokens.HashToken(refreshToken),
            CreatedAt = now,
            LastSeenAt = now,
            ExpiresAt = now.AddDays(SessionLifetimeDays)
        };

        await _repository.AddSessionAsync(session, cancellationToken);
        return await ToSessionDtoAsync(accessToken, refreshToken, session, account, device, cancellationToken);
    }

    private async Task<AuthSessionDto> RotateSessionAsync(
        AuthSession session,
        Account account,
        DeviceBinding device,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var accessToken = _tokens.CreateToken();
        var refreshToken = _tokens.CreateToken();
        session.AccessTokenHash = _tokens.HashToken(accessToken);
        session.RefreshTokenHash = _tokens.HashToken(refreshToken);
        session.DeviceId = device.Id;
        session.LastSeenAt = now;
        session.ExpiresAt = now.AddDays(SessionLifetimeDays);
        await _repository.UpdateSessionAsync(session, cancellationToken);

        return await ToSessionDtoAsync(accessToken, refreshToken, session, account, device, cancellationToken);
    }

    private async Task<AuthSessionDto> ToSessionDtoAsync(
        string accessToken,
        string refreshToken,
        AuthSession session,
        Account account,
        DeviceBinding device,
        CancellationToken cancellationToken)
    {
        return new AuthSessionDto(
            accessToken,
            refreshToken,
            session.ExpiresAt,
            ToAccountDto(account),
            ToDeviceDto(device),
            await GetLicenseStatusAsync(account.Id, cancellationToken));
    }

    private async Task<PrincipalEntities?> ValidateAccessTokenInternalAsync(
        string? accessToken,
        CancellationToken cancellationToken)
    {
        var token = NormalizeOptional(accessToken);
        if (token is null)
        {
            return null;
        }

        var now = _clock.Now;
        var session = await _repository.FindSessionByAccessTokenHashAsync(_tokens.HashToken(token), cancellationToken);
        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= now)
        {
            return null;
        }

        var account = await _repository.GetAccountAsync(session.AccountId, cancellationToken);
        var device = await _repository.GetDeviceAsync(session.DeviceId, cancellationToken);
        if (account is null || device is null || account.Status != AccountStatus.Active)
        {
            return null;
        }

        session.LastSeenAt = now;
        await _repository.UpdateSessionAsync(session, cancellationToken);
        return new PrincipalEntities(session, account, device);
    }

    private async Task<AuthSession?> FindSessionAsync(string? accessToken, CancellationToken cancellationToken)
    {
        var token = NormalizeOptional(accessToken);
        if (token is null)
        {
            return null;
        }

        return await _repository.FindSessionByAccessTokenHashAsync(_tokens.HashToken(token), cancellationToken);
    }

    private async Task<DeviceBinding> BindDeviceInternalAsync(
        Account account,
        string? deviceFingerprint,
        string? deviceName,
        int? maxTrustedDevices,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var fingerprint = NormalizeOptional(deviceFingerprint) ?? "milu-local-device";
        var fingerprintHash = _tokens.HashSecret($"{account.Id}:{fingerprint}");
        var device = await _repository.FindDeviceAsync(account.Id, fingerprintHash, cancellationToken);
        if (device is not null)
        {
            if (!device.Trusted)
            {
                await EnsureDeviceSlotAvailableAsync(account.Id, maxTrustedDevices, cancellationToken);
            }

            device.DeviceName = NormalizeOptional(deviceName) ?? device.DeviceName;
            device.LastSeenAt = now;
            device.Trusted = true;
            await _repository.UpdateDeviceAsync(device, cancellationToken);
            return device;
        }

        await EnsureDeviceSlotAvailableAsync(account.Id, maxTrustedDevices, cancellationToken);

        device = new DeviceBinding
        {
            Id = CreateId("dev"),
            AccountId = account.Id,
            MachineFingerprintHash = fingerprintHash,
            DeviceName = NormalizeOptional(deviceName) ?? "本机设备",
            FirstSeenAt = now,
            LastSeenAt = now,
            Trusted = true
        };

        await _repository.AddDeviceAsync(device, cancellationToken);
        return device;
    }

    private async Task EnsureDeviceSlotAvailableAsync(
        string accountId,
        int? maxTrustedDevices,
        CancellationToken cancellationToken)
    {
        if (maxTrustedDevices is null)
        {
            return;
        }

        var trustedDevices = await _repository.CountTrustedDevicesAsync(accountId, cancellationToken);
        if (trustedDevices >= maxTrustedDevices)
        {
            throw AuthCommandException.Forbidden("device_limit_exceeded", $"当前许可证最多绑定 {maxTrustedDevices} 台设备。");
        }
    }

    private async Task ActivateInternalAsync(
        Account account,
        DeviceBinding device,
        string? activationCode,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var code = NormalizeOptional(activationCode)
            ?? throw AuthCommandException.BadRequest("activation_code_required", "请填写测试激活码。");
        var decision = await _licensingAdapter.ValidateActivationCodeAsync(code, account, device, now, cancellationToken);
        if (!decision.Accepted)
        {
            throw AuthCommandException.Forbidden(decision.Code, decision.Message);
        }

        var trustedDevices = await _repository.CountTrustedDevicesAsync(account.Id, cancellationToken);
        if (trustedDevices > decision.MaxDevices)
        {
            device.Trusted = false;
            await _repository.UpdateDeviceAsync(device, cancellationToken);
            throw AuthCommandException.Forbidden("device_limit_exceeded", $"当前许可证最多绑定 {decision.MaxDevices} 台设备。");
        }

        var existing = await _repository.GetLatestLicenseAsync(account.Id, cancellationToken);
        if (existing is null)
        {
            var license = new LicenseGrant
            {
                Id = CreateId("lic"),
                AccountId = account.Id,
                LicenseType = decision.LicenseType,
                Plan = decision.Plan,
                ActivationCodeHash = _tokens.HashSecret(code),
                Status = LicenseStatus.Active,
                StartsAt = now,
                ExpiresAt = now.AddDays(decision.ValidDays),
                MaxDevices = decision.MaxDevices,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _repository.AddLicenseAsync(license, cancellationToken);
            return;
        }

        existing.LicenseType = decision.LicenseType;
        existing.Plan = decision.Plan;
        existing.ActivationCodeHash = _tokens.HashSecret(code);
        existing.Status = LicenseStatus.Active;
        existing.StartsAt = now;
        existing.ExpiresAt = now.AddDays(decision.ValidDays);
        existing.MaxDevices = decision.MaxDevices;
        existing.UpdatedAt = now;
        await _repository.UpdateLicenseAsync(existing, cancellationToken);
    }

    private async Task<LicenseStatusDto> GetLicenseStatusAsync(string accountId, CancellationToken cancellationToken)
    {
        var now = _clock.Now;
        var active = await _repository.GetActiveLicenseAsync(accountId, now, cancellationToken);
        if (active is not null)
        {
            return ToLicenseDto(active, "许可证有效。", active: true);
        }

        var latest = await _repository.GetLatestLicenseAsync(accountId, cancellationToken);
        if (latest is null)
        {
            return MissingLicense("账号尚未激活，请输入测试激活码。");
        }

        if (latest.Status == LicenseStatus.Revoked)
        {
            return ToLicenseDto(latest, "许可证已被撤销，请重新激活。", active: false);
        }

        if (latest.ExpiresAt <= now)
        {
            return ToLicenseDto(latest, "许可证已过期，请重新激活。", active: false, status: "expired");
        }

        return ToLicenseDto(latest, "许可证当前不可用，请重新激活。", active: false);
    }

    private static AuthenticatedPrincipal ToPrincipal(PrincipalEntities principal)
    {
        return new AuthenticatedPrincipal(
            new AccountSnapshot(
                principal.AccountEntity.Id,
                principal.AccountEntity.DisplayName,
                principal.AccountEntity.Email,
                principal.AccountEntity.Phone),
            new DeviceSnapshot(principal.DeviceEntity.Id, principal.DeviceEntity.DeviceName),
            principal.Session.Id);
    }

    private static AuthAccountDto ToAccountDto(Account account)
    {
        return new AuthAccountDto(
            account.Id,
            account.Email,
            account.Phone,
            account.DisplayName,
            AccountStatusValue(account.Status),
            account.CreatedAt,
            account.LastLoginAt);
    }

    private static AuthDeviceDto ToDeviceDto(DeviceBinding device)
    {
        return new AuthDeviceDto(
            device.Id,
            device.DeviceName,
            device.Trusted,
            device.FirstSeenAt,
            device.LastSeenAt);
    }

    private static LicenseStatusDto MissingLicense(string message)
    {
        return new LicenseStatusDto("missing", false, MissingLicensePlan, "none", null, null, 0, message);
    }

    private static LicenseStatusDto ToLicenseDto(
        LicenseGrant license,
        string message,
        bool active,
        string? status = null)
    {
        return new LicenseStatusDto(
            status ?? LicenseStatusValue(license.Status),
            active,
            license.Plan,
            LicenseKindValue(license.LicenseType),
            license.StartsAt,
            license.ExpiresAt,
            license.MaxDevices,
            message);
    }

    private static string NormalizePassword(string? password)
    {
        var normalized = NormalizeOptional(password);
        if (normalized is null || normalized.Length < MinimumPasswordLength)
        {
            throw AuthCommandException.BadRequest("weak_password", $"密码至少需要 {MinimumPasswordLength} 个字符。");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string CreateId(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }

    private static string AccountStatusValue(AccountStatus status)
    {
        return status switch
        {
            AccountStatus.Locked => "locked",
            AccountStatus.Deleted => "deleted",
            _ => "active"
        };
    }

    private static string LicenseKindValue(LicenseKind kind)
    {
        return kind switch
        {
            LicenseKind.Trial => "trial",
            LicenseKind.OfflineSigned => "offline_signed",
            _ => "paid"
        };
    }

    private static string LicenseStatusValue(LicenseStatus status)
    {
        return status switch
        {
            LicenseStatus.Expired => "expired",
            LicenseStatus.Revoked => "revoked",
            _ => "active"
        };
    }

    private sealed record PrincipalEntities(AuthSession Session, Account AccountEntity, DeviceBinding DeviceEntity)
    {
        public AccountSnapshot Account => new(AccountEntity.Id, AccountEntity.DisplayName, AccountEntity.Email, AccountEntity.Phone);
    }
}

public sealed class AuthCommandException : Exception
{
    private AuthCommandException(int statusCode, string code, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }

    public int StatusCode { get; }

    public string Code { get; }

    public static AuthCommandException BadRequest(string code, string message) => new(400, code, message);

    public static AuthCommandException Unauthorized(string code, string message) => new(401, code, message);

    public static AuthCommandException Forbidden(string code, string message) => new(403, code, message);

    public static AuthCommandException Conflict(string code, string message) => new(409, code, message);
}
