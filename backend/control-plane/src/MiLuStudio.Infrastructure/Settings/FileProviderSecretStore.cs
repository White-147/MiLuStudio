namespace MiLuStudio.Infrastructure.Settings;

using Convert = global::System.Convert;
using Encoding = global::System.Text.Encoding;
using JsonSerializer = global::System.Text.Json.JsonSerializer;
using JsonSerializerDefaults = global::System.Text.Json.JsonSerializerDefaults;
using JsonSerializerOptions = global::System.Text.Json.JsonSerializerOptions;
using SHA256 = global::System.Security.Cryptography.SHA256;
using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Settings;
using MiLuStudio.Infrastructure.Configuration;

public sealed class FileProviderSecretStore : IProviderSecretStore
{
    private const string StoreMode = "stage22_metadata_only";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _storePath;

    public FileProviderSecretStore(IOptions<ControlPlaneOptions> options)
    {
        var configuredPath = options.Value.ProviderSecretStorePath;
        _storePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(options.Value.StorageRoot, "settings", "provider-secrets.local.json")
            : configuredPath;
    }

    public async Task<IReadOnlyDictionary<string, ProviderSecretDescriptorState>> ListAsync(
        CancellationToken cancellationToken)
    {
        var state = await LoadAsync(cancellationToken);
        return state.Secrets.ToDictionary(secret => secret.Kind, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<ProviderSecretDescriptorState> SaveMetadataAsync(
        string kind,
        string secret,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var normalizedKind = kind.Trim().ToLowerInvariant();
        var descriptor = new ProviderSecretDescriptorState(
            normalizedKind,
            $"provider-secret:{normalizedKind}",
            MaskSecret(secret.Trim()),
            FingerprintSecret(secret.Trim()),
            now,
            RawSecretPersisted: false,
            UsableForProviderCalls: false,
            StoreMode);

        var state = await LoadAsync(cancellationToken);
        var secrets = state.Secrets
            .Where(existing => !string.Equals(existing.Kind, normalizedKind, StringComparison.OrdinalIgnoreCase))
            .Append(descriptor)
            .OrderBy(existing => existing.Kind, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await SaveAsync(new ProviderSecretStoreState(now, secrets), cancellationToken);
        return descriptor;
    }

    public async Task ClearAsync(string kind, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var normalizedKind = kind.Trim().ToLowerInvariant();
        var state = await LoadAsync(cancellationToken);
        var secrets = state.Secrets
            .Where(existing => !string.Equals(existing.Kind, normalizedKind, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        await SaveAsync(new ProviderSecretStoreState(now, secrets), cancellationToken);
    }

    public Task<ProviderSecretStoreStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_storePath);
        var metadataStoreAvailable = string.IsNullOrWhiteSpace(directory) || Directory.Exists(directory);
        var checks = new List<string>
        {
            "raw_secret_persistence=disabled",
            "provider_call_secret_material=unavailable",
            "secret_metadata_file=project_storage_scope"
        };

        if (!metadataStoreAvailable)
        {
            checks.Add("metadata_directory_will_be_created_on_save");
            metadataStoreAvailable = true;
        }

        var status = new ProviderSecretStoreStatusDto(
            StoreMode,
            "project_local_metadata",
            metadataStoreAvailable,
            RawSecretPersistenceAllowed: false,
            ProviderCallSecretsAvailable: false,
            checks);
        return Task.FromResult(status);
    }

    private async Task<ProviderSecretStoreState> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_storePath))
        {
            return new ProviderSecretStoreState(DateTimeOffset.MinValue, Array.Empty<ProviderSecretDescriptorState>());
        }

        await using var stream = File.OpenRead(_storePath);
        var state = await JsonSerializer.DeserializeAsync<ProviderSecretStoreState>(stream, JsonOptions, cancellationToken);
        return state ?? new ProviderSecretStoreState(DateTimeOffset.MinValue, Array.Empty<ProviderSecretDescriptorState>());
    }

    private async Task SaveAsync(ProviderSecretStoreState state, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_storePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{_storePath}.{Guid.NewGuid():N}.tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
        }

        File.Move(tempPath, _storePath, true);
    }

    private static string MaskSecret(string secret)
    {
        if (secret.Length <= 4)
        {
            return "****";
        }

        return $"****{secret[^4..]}";
    }

    private static string FingerprintSecret(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return $"sha256:{Convert.ToHexString(bytes)[..16].ToLowerInvariant()}";
    }
}
