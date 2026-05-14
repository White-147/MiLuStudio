namespace MiLuStudio.Infrastructure.Settings;

using Convert = global::System.Convert;
using Encoding = global::System.Text.Encoding;
using JsonSerializer = global::System.Text.Json.JsonSerializer;
using JsonSerializerDefaults = global::System.Text.Json.JsonSerializerDefaults;
using JsonSerializerOptions = global::System.Text.Json.JsonSerializerOptions;
using ProtectedData = global::System.Security.Cryptography.ProtectedData;
using DataProtectionScope = global::System.Security.Cryptography.DataProtectionScope;
using SHA256 = global::System.Security.Cryptography.SHA256;
using SupportedOSPlatformGuardAttribute = global::System.Runtime.Versioning.SupportedOSPlatformGuardAttribute;
using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Settings;
using MiLuStudio.Infrastructure.Configuration;

public sealed class FileProviderSecretStore : IProviderSecretStore
{
    private const string StoreMode = "stage23_windows_dpapi";
    private const string ProtectionMode = "windows_current_user_dpapi";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("MiLuStudio.ProviderSecret.v1");

    private readonly string _storePath;
    private readonly string _vaultPath;

    public FileProviderSecretStore(IOptions<ControlPlaneOptions> options)
    {
        var configuredPath = options.Value.ProviderSecretStorePath;
        _storePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(options.Value.StorageRoot, "settings", "provider-secrets.local.json")
            : configuredPath;
        _vaultPath = Path.Combine(Path.GetDirectoryName(_storePath) ?? options.Value.StorageRoot, "provider-secrets.vault.local.json");
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
            RawSecretPersisted: CanProtectSecrets(),
            UsableForProviderCalls: CanProtectSecrets(),
            StoreMode);

        var state = await LoadAsync(cancellationToken);
        var secrets = state.Secrets
            .Where(existing => !string.Equals(existing.Kind, normalizedKind, StringComparison.OrdinalIgnoreCase))
            .Append(descriptor)
            .OrderBy(existing => existing.Kind, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await SaveAsync(new ProviderSecretStoreState(now, secrets), cancellationToken);
        await SaveVaultSecretAsync(normalizedKind, secret.Trim(), now, cancellationToken);
        return descriptor;
    }

    public async Task<string?> GetSecretAsync(string kind, CancellationToken cancellationToken)
    {
        if (!CanProtectSecrets())
        {
            return null;
        }

        var normalizedKind = kind.Trim().ToLowerInvariant();
        var vault = await LoadVaultAsync(cancellationToken);
        var secret = vault.Secrets.FirstOrDefault(entry =>
            string.Equals(entry.Kind, normalizedKind, StringComparison.OrdinalIgnoreCase));
        if (secret is null || string.IsNullOrWhiteSpace(secret.ProtectedSecretBase64))
        {
            return null;
        }

        try
        {
            var protectedBytes = Convert.FromBase64String(secret.ProtectedSecretBase64);
            var plainBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearAsync(string kind, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var normalizedKind = kind.Trim().ToLowerInvariant();
        var state = await LoadAsync(cancellationToken);
        var secrets = state.Secrets
            .Where(existing => !string.Equals(existing.Kind, normalizedKind, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        await SaveAsync(new ProviderSecretStoreState(now, secrets), cancellationToken);
        await ClearVaultSecretAsync(normalizedKind, now, cancellationToken);
    }

    public Task<ProviderSecretStoreStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_storePath);
        var metadataStoreAvailable = string.IsNullOrWhiteSpace(directory) || Directory.Exists(directory);
        var checks = new List<string>
        {
            $"raw_secret_persistence={(CanProtectSecrets() ? "enabled" : "disabled")}",
            $"provider_call_secret_material={(CanProtectSecrets() ? "available" : "unavailable")}",
            "secret_metadata_file=project_storage_scope",
            "secret_material=windows_current_user_dpapi"
        };

        if (!metadataStoreAvailable)
        {
            checks.Add("metadata_directory_will_be_created_on_save");
            metadataStoreAvailable = true;
        }

        var status = new ProviderSecretStoreStatusDto(
            StoreMode,
            "project_local_dpapi",
            metadataStoreAvailable,
            RawSecretPersistenceAllowed: CanProtectSecrets(),
            ProviderCallSecretsAvailable: CanProtectSecrets(),
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

    private async Task<ProviderSecretVaultState> LoadVaultAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_vaultPath))
        {
            return new ProviderSecretVaultState(DateTimeOffset.MinValue, Array.Empty<ProviderSecretMaterialState>());
        }

        await using var stream = File.OpenRead(_vaultPath);
        var state = await JsonSerializer.DeserializeAsync<ProviderSecretVaultState>(stream, JsonOptions, cancellationToken);
        return state ?? new ProviderSecretVaultState(DateTimeOffset.MinValue, Array.Empty<ProviderSecretMaterialState>());
    }

    private async Task SaveVaultAsync(ProviderSecretVaultState state, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_vaultPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{_vaultPath}.{Guid.NewGuid():N}.tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
        }

        File.Move(tempPath, _vaultPath, true);
    }

    private async Task SaveVaultSecretAsync(
        string normalizedKind,
        string secret,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (!CanProtectSecrets())
        {
            return;
        }

        var plainBytes = Encoding.UTF8.GetBytes(secret);
        var protectedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
        var material = new ProviderSecretMaterialState(
            normalizedKind,
            Convert.ToBase64String(protectedBytes),
            ProtectionMode,
            now);

        var vault = await LoadVaultAsync(cancellationToken);
        var secrets = vault.Secrets
            .Where(existing => !string.Equals(existing.Kind, normalizedKind, StringComparison.OrdinalIgnoreCase))
            .Append(material)
            .OrderBy(existing => existing.Kind, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await SaveVaultAsync(new ProviderSecretVaultState(now, secrets), cancellationToken);
    }

    private async Task ClearVaultSecretAsync(
        string normalizedKind,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var vault = await LoadVaultAsync(cancellationToken);
        var secrets = vault.Secrets
            .Where(existing => !string.Equals(existing.Kind, normalizedKind, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        await SaveVaultAsync(new ProviderSecretVaultState(now, secrets), cancellationToken);
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

    [SupportedOSPlatformGuard("windows")]
    private static bool CanProtectSecrets() => OperatingSystem.IsWindows();
}

internal sealed record ProviderSecretVaultState(
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ProviderSecretMaterialState> Secrets);

internal sealed record ProviderSecretMaterialState(
    string Kind,
    string ProtectedSecretBase64,
    string ProtectionMode,
    DateTimeOffset UpdatedAt);
