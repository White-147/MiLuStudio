namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Application.Settings;

public interface IProviderSettingsRepository
{
    Task<ProviderSettingsState?> GetAsync(CancellationToken cancellationToken);

    Task SaveAsync(ProviderSettingsState settings, CancellationToken cancellationToken);
}
