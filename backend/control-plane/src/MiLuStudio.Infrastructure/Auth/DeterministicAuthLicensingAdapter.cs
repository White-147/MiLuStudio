namespace MiLuStudio.Infrastructure.Auth;

using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;
using MiLuStudio.Infrastructure.Configuration;

public sealed class DeterministicAuthLicensingAdapter : IAuthLicensingAdapter
{
    private readonly ControlPlaneOptions _options;

    public DeterministicAuthLicensingAdapter(IOptions<ControlPlaneOptions> options)
    {
        _options = options.Value;
    }

    public Task<LicenseActivationDecision> ValidateActivationCodeAsync(
        string activationCode,
        Account account,
        DeviceBinding device,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var normalized = activationCode.Trim();
        if (!string.Equals(normalized, _options.AuthTestActivationCode, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new LicenseActivationDecision(
                false,
                "invalid_activation_code",
                "测试激活码无效。Stage 16 本地 adapter 只接受配置中的测试激活码。",
                LicenseKind.Paid,
                "stage16-local",
                _options.AuthLicenseValidDays,
                _options.AuthMaxDevices));
        }

        return Task.FromResult(new LicenseActivationDecision(
            true,
            "accepted",
            "测试激活码已通过本地 deterministic adapter 校验。",
            LicenseKind.Paid,
            "stage16-local",
            _options.AuthLicenseValidDays,
            _options.AuthMaxDevices));
    }
}
