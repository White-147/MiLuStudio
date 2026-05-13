namespace MiLuStudio.Infrastructure.System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.System;
using MiLuStudio.Infrastructure.Configuration;
using MiLuStudio.Infrastructure.Persistence.PostgreSql;

public sealed class PostgreSqlControlPlanePreflightService : IControlPlanePreflightService
{
    private readonly MiLuStudioDbContext _db;
    private readonly IControlPlaneMigrationService _migrations;
    private readonly ControlPlaneOptions _options;

    public PostgreSqlControlPlanePreflightService(
        MiLuStudioDbContext db,
        IControlPlaneMigrationService migrations,
        IOptions<ControlPlaneOptions> options)
    {
        _db = db;
        _migrations = migrations;
        _options = options.Value;
    }

    public async Task<ControlPlanePreflightDto> CheckAsync(CancellationToken cancellationToken)
    {
        var checks = new List<PreflightCheckDto>
        {
            new(
                "repository_provider",
                "ok",
                "Control API is configured for PostgreSQL repositories.",
                new Dictionary<string, string> { ["provider"] = RepositoryProviderNames.PostgreSql })
        };
        var recommendations = new List<string>();
        var healthy = true;

        var connectionString = _db.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            healthy = false;
            checks.Add(new(
                "connection_string",
                "error",
                "ConnectionStrings:MiLuStudioControlPlane is missing.",
                new Dictionary<string, string>()));
            recommendations.Add("Configure ConnectionStrings:MiLuStudioControlPlane in the backend appsettings or environment.");
        }
        else
        {
            checks.Add(new(
                "connection_string",
                "ok",
                "PostgreSQL connection string is configured.",
                new Dictionary<string, string> { ["connectionString"] = RedactConnectionString(connectionString) }));
        }

        var canConnect = false;
        try
        {
            canConnect = await _db.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception error) when (error is InvalidOperationException or TimeoutException or Npgsql.NpgsqlException)
        {
            checks.Add(new(
                "database_reachable",
                "error",
                $"PostgreSQL is not reachable: {error.Message}",
                new Dictionary<string, string>()));
        }

        if (canConnect)
        {
            checks.Add(new(
                "database_reachable",
                "ok",
                "PostgreSQL is reachable from the backend process.",
                new Dictionary<string, string>()));
        }
        else
        {
            healthy = false;
            recommendations.Add("Start PostgreSQL and verify host, port, database, username, and password in backend configuration.");
        }

        if (canConnect)
        {
            var migrationStatus = await _migrations.GetStatusAsync(cancellationToken);
            var pendingCount = migrationStatus.Migrations.Count(migration => migration.Status == "pending");
            var migrationHealth = migrationStatus.Status == "up_to_date" ? "ok" : "warning";
            checks.Add(new(
                "migrations",
                migrationHealth,
                pendingCount == 0 ? "All SQL migrations are applied." : $"{pendingCount} SQL migration(s) are pending.",
                new Dictionary<string, string>
                {
                    ["status"] = migrationStatus.Status,
                    ["pendingCount"] = pendingCount.ToString()
                }));

            if (pendingCount > 0)
            {
                healthy = false;
                recommendations.Add("Run the backend migration apply endpoint or an equivalent backend migration runner before using PostgreSQL provider.");
            }
        }

        var storageExists = Directory.Exists(_options.StorageRoot);
        checks.Add(new(
            "storage_root",
            storageExists ? "ok" : "warning",
            storageExists ? "Storage root exists." : "Storage root does not exist yet; backend setup should create it before real asset writes.",
            new Dictionary<string, string> { ["storageRoot"] = _options.StorageRoot }));

        return new ControlPlanePreflightDto(
            RepositoryProviderNames.PostgreSql,
            healthy,
            checks,
            recommendations);
    }

    private static string RedactConnectionString(string connectionString)
    {
        return string.Join(
            ";",
            connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ? "Password=<redacted>" : part));
    }
}
