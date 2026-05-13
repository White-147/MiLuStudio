namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public interface IAuthLicensingAdapter
{
    Task<LicenseActivationDecision> ValidateActivationCodeAsync(
        string activationCode,
        Account account,
        DeviceBinding device,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}

public sealed record LicenseActivationDecision(
    bool Accepted,
    string Code,
    string Message,
    LicenseKind LicenseType,
    string Plan,
    int ValidDays,
    int MaxDevices);
