namespace MiLuStudio.Infrastructure.System;

using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.System;
using MiLuStudio.Infrastructure.Configuration;

public sealed class InMemoryControlPlanePreflightService : IControlPlanePreflightService, IControlPlaneMigrationService
{
    private readonly ControlPlaneOptions _options;

    public InMemoryControlPlanePreflightService(IOptions<ControlPlaneOptions> options)
    {
        _options = options.Value;
    }

    public Task<ControlPlanePreflightDto> CheckAsync(CancellationToken cancellationToken)
    {
        var storageStatus = Directory.Exists(_options.StorageRoot) ? "ok" : "warning";
        var checks = new List<PreflightCheckDto>
        {
            new(
                "repository_provider",
                "ok",
                "Control API is using the in-memory repository provider.",
                new Dictionary<string, string> { ["provider"] = RepositoryProviderNames.InMemory }),
            new(
                "database",
                "skipped",
                "PostgreSQL is not required while RepositoryProvider=InMemory.",
                new Dictionary<string, string>()),
            new(
                "migrations",
                "skipped",
                "SQL migrations are checked only when RepositoryProvider=PostgreSQL.",
                new Dictionary<string, string> { ["migrationsPath"] = _options.MigrationsPath }),
            new(
                "storage_root",
                storageStatus,
                Directory.Exists(_options.StorageRoot) ? "Storage root exists." : "Storage root does not exist yet; backend setup should create it before real asset writes.",
                new Dictionary<string, string> { ["storageRoot"] = _options.StorageRoot })
        };

        return Task.FromResult(new ControlPlanePreflightDto(
            RepositoryProviderNames.InMemory,
            Healthy: true,
            checks,
            ["Switch ControlPlane:RepositoryProvider to PostgreSQL only after local database configuration is ready."]));
    }

    public Task<MigrationStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new MigrationStatusDto(
            RepositoryProviderNames.InMemory,
            "skipped",
            []));
    }

    public Task<MigrationApplyResultDto> ApplyPendingAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new MigrationApplyResultDto(
            RepositoryProviderNames.InMemory,
            "skipped",
            [],
            []));
    }
}
