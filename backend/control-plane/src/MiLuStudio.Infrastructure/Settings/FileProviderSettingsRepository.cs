namespace MiLuStudio.Infrastructure.Settings;

using JsonSerializer = global::System.Text.Json.JsonSerializer;
using JsonSerializerDefaults = global::System.Text.Json.JsonSerializerDefaults;
using JsonSerializerOptions = global::System.Text.Json.JsonSerializerOptions;
using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Settings;
using MiLuStudio.Infrastructure.Configuration;

public sealed class FileProviderSettingsRepository : IProviderSettingsRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;

    public FileProviderSettingsRepository(IOptions<ControlPlaneOptions> options)
    {
        var configuredPath = options.Value.ProviderSettingsPath;
        _settingsPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(options.Value.StorageRoot, "settings", "provider-adapters.local.json")
            : configuredPath;
    }

    public async Task<ProviderSettingsState?> GetAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_settingsPath))
        {
            return null;
        }

        await using var stream = File.OpenRead(_settingsPath);
        return await JsonSerializer.DeserializeAsync<ProviderSettingsState>(stream, JsonOptions, cancellationToken);
    }

    public async Task SaveAsync(ProviderSettingsState settings, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{_settingsPath}.{Guid.NewGuid():N}.tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
        }

        File.Move(tempPath, _settingsPath, true);
    }
}
