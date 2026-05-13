namespace MiLuStudio.Infrastructure.System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.System;
using MiLuStudio.Infrastructure.Configuration;
using MiLuStudio.Infrastructure.Persistence.PostgreSql;
using DbCommand = global::System.Data.Common.DbCommand;
using DbConnection = global::System.Data.Common.DbConnection;
using DbException = global::System.Data.Common.DbException;
using DbTransaction = global::System.Data.Common.DbTransaction;

public sealed class PostgreSqlMigrationService : IControlPlaneMigrationService
{
    private readonly MiLuStudioDbContext _db;
    private readonly ControlPlaneOptions _options;

    public PostgreSqlMigrationService(MiLuStudioDbContext db, IOptions<ControlPlaneOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<MigrationStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        var migrationFiles = GetMigrationFiles();
        var applied = await TryGetAppliedMigrationsAsync(cancellationToken);

        var status = applied is null
            ? "unreachable"
            : migrationFiles.All(file => applied.ContainsKey(file.Id))
                ? "up_to_date"
                : "pending";

        var migrations = migrationFiles
            .Select(file => new MigrationFileDto(
                file.Id,
                file.FileName,
                applied?.ContainsKey(file.Id) == true ? "applied" : "pending",
                applied?.TryGetValue(file.Id, out var appliedAt) == true ? appliedAt : null))
            .ToList();

        return new MigrationStatusDto(RepositoryProviderNames.PostgreSql, status, migrations);
    }

    public async Task<MigrationApplyResultDto> ApplyPendingAsync(CancellationToken cancellationToken)
    {
        var migrationFiles = GetMigrationFiles();
        var applied = await GetAppliedMigrationsAsync(cancellationToken);
        var appliedIds = new List<string>();
        var skippedIds = new List<string>();

        foreach (var migration in migrationFiles)
        {
            if (applied.ContainsKey(migration.Id))
            {
                skippedIds.Add(migration.Id);
                continue;
            }

            await ApplyMigrationAsync(migration, cancellationToken);
            appliedIds.Add(migration.Id);
        }

        return new MigrationApplyResultDto(
            RepositoryProviderNames.PostgreSql,
            appliedIds.Count == 0 ? "up_to_date" : "applied",
            appliedIds,
            skippedIds);
    }

    private async Task<IReadOnlyDictionary<string, DateTimeOffset>?> TryGetAppliedMigrationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await GetAppliedMigrationsAsync(cancellationToken, createTable: false);
        }
        catch (DbException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private Task<IReadOnlyDictionary<string, DateTimeOffset>> GetAppliedMigrationsAsync(CancellationToken cancellationToken)
    {
        return GetAppliedMigrationsAsync(cancellationToken, createTable: true);
    }

    private async Task<IReadOnlyDictionary<string, DateTimeOffset>> GetAppliedMigrationsAsync(
        CancellationToken cancellationToken,
        bool createTable)
    {
        var connection = _db.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);

        if (createTable)
        {
            await ExecuteNonQueryAsync(
                connection,
                """
                create table if not exists schema_migrations (
                    migration_id text primary key,
                    applied_at timestamptz not null
                );
                """,
                cancellationToken);
        }
        else
        {
            var exists = await ExecuteScalarAsync(connection, "select to_regclass('public.schema_migrations') is not null;", cancellationToken);
            if (exists is not bool tableExists || !tableExists)
            {
                return new Dictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);
            }
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "select migration_id, applied_at from schema_migrations order by migration_id;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var applied = new Dictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            applied[reader.GetString(0)] = reader.GetFieldValue<DateTimeOffset>(1);
        }

        return applied;
    }

    private async Task ApplyMigrationAsync(MigrationFile migration, CancellationToken cancellationToken)
    {
        var connection = _db.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await ExecuteNonQueryAsync(connection, migration.Sql, cancellationToken, transaction);
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = "insert into schema_migrations (migration_id, applied_at) values (@migration_id, @applied_at);";
            AddParameter(insert, "migration_id", migration.Id);
            AddParameter(insert, "applied_at", DateTimeOffset.UtcNow);
            await insert.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private IReadOnlyList<MigrationFile> GetMigrationFiles()
    {
        var directory = ResolveMigrationsPath(_options.MigrationsPath);
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.GetFiles(directory, "*.sql")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => new MigrationFile(
                Path.GetFileNameWithoutExtension(path),
                Path.GetFileName(path),
                File.ReadAllText(path)))
            .ToList();
    }

    private static string ResolveMigrationsPath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, configuredPath);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return Path.GetFullPath(configuredPath);
    }

    private static async Task OpenIfNeededAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection.State != global::System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }
    }

    private static async Task ExecuteNonQueryAsync(
        DbConnection connection,
        string sql,
        CancellationToken cancellationToken,
        DbTransaction? transaction = null)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<object?> ExecuteScalarAsync(
        DbConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return await command.ExecuteScalarAsync(cancellationToken);
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private sealed record MigrationFile(string Id, string FileName, string Sql);
}
