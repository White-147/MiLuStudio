namespace MiLuStudio.Infrastructure.System;

using Microsoft.EntityFrameworkCore;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.System;
using MiLuStudio.Infrastructure.Configuration;
using MiLuStudio.Infrastructure.Persistence.Sqlite;

public sealed class SqliteMigrationService : IControlPlaneMigrationService
{
    private const string SchemaId = "sqlite_efcore_schema";
    private const string SchemaFileName = "efcore_ensure_created";

    private static readonly string[] RequiredTables =
    [
        "accounts",
        "assets",
        "auth_sessions",
        "characters",
        "cost_ledger",
        "devices",
        "generation_tasks",
        "licenses",
        "production_jobs",
        "projects",
        "shots",
        "story_inputs"
    ];

    private readonly MiLuStudioDbContext _db;

    public SqliteMigrationService(MiLuStudioDbContext db)
    {
        _db = db;
    }

    public async Task<MigrationStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        var canConnect = await CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return new MigrationStatusDto(
                RepositoryProviderNames.Sqlite,
                "unreachable",
                [new MigrationFileDto(SchemaId, SchemaFileName, "pending", null)]);
        }

        var existingTables = await GetExistingTablesAsync(cancellationToken);
        var missingTables = RequiredTables
            .Where(table => !existingTables.Contains(table))
            .ToArray();

        return new MigrationStatusDto(
            RepositoryProviderNames.Sqlite,
            missingTables.Length == 0 ? "up_to_date" : "pending",
            [new MigrationFileDto(SchemaId, SchemaFileName, missingTables.Length == 0 ? "applied" : "pending", null)]);
    }

    public async Task<MigrationApplyResultDto> ApplyPendingAsync(CancellationToken cancellationToken)
    {
        var before = await GetStatusAsync(cancellationToken);
        await _db.Database.EnsureCreatedAsync(cancellationToken);
        var after = await GetStatusAsync(cancellationToken);

        return new MigrationApplyResultDto(
            RepositoryProviderNames.Sqlite,
            after.Status == "up_to_date" && before.Status != "up_to_date" ? "applied" : after.Status,
            after.Status == "up_to_date" && before.Status != "up_to_date" ? [SchemaId] : [],
            before.Status == "up_to_date" ? [SchemaId] : []);
    }

    private async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _db.Database.CanConnectAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private async Task<HashSet<string>> GetExistingTablesAsync(CancellationToken cancellationToken)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != global::System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            select name
            from sqlite_master
            where type = 'table'
              and name not like 'sqlite_%'
            order by name;
            """;

        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
