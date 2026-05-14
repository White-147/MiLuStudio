namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Application.Settings;

public interface IProviderSecretStore
{
    Task<IReadOnlyDictionary<string, ProviderSecretDescriptorState>> ListAsync(CancellationToken cancellationToken);

    Task<ProviderSecretDescriptorState> SaveMetadataAsync(
        string kind,
        string secret,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task ClearAsync(string kind, DateTimeOffset now, CancellationToken cancellationToken);

    Task<ProviderSecretStoreStatusDto> GetStatusAsync(CancellationToken cancellationToken);
}
