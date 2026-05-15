namespace MiLuStudio.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Infrastructure.Assets;
using MiLuStudio.Infrastructure.Auth;
using MiLuStudio.Infrastructure.Configuration;
using MiLuStudio.Infrastructure.Persistence.InMemory;
using MiLuStudio.Infrastructure.Persistence.Sqlite;
using MiLuStudio.Infrastructure.Settings;
using MiLuStudio.Infrastructure.Skills;
using MiLuStudio.Infrastructure.System;
using MiLuStudio.Infrastructure.Time;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMiLuStudioInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = LoadOptions(configuration);
        services.Configure<ControlPlaneOptions>(configured =>
        {
            configured.RepositoryProvider = options.RepositoryProvider;
            configured.MigrationsPath = options.MigrationsPath;
            configured.StorageRoot = options.StorageRoot;
            configured.UploadsRoot = options.UploadsRoot;
            configured.FfmpegBinPath = options.FfmpegBinPath;
            configured.OcrTesseractPath = options.OcrTesseractPath;
            configured.OcrTessdataPath = options.OcrTessdataPath;
            configured.OcrLanguages = options.OcrLanguages;
            configured.AssetParseTimeoutSeconds = options.AssetParseTimeoutSeconds;
            configured.AssetTranscodeTimeoutSeconds = options.AssetTranscodeTimeoutSeconds;
            configured.AssetVideoFrameLimit = options.AssetVideoFrameLimit;
            configured.OcrTimeoutSeconds = options.OcrTimeoutSeconds;
            configured.ProviderSettingsPath = options.ProviderSettingsPath;
            configured.ProviderSecretStorePath = options.ProviderSecretStorePath;
            configured.WorkerId = options.WorkerId;
            configured.PythonExecutablePath = options.PythonExecutablePath;
            configured.PythonSkillsRoot = options.PythonSkillsRoot;
            configured.SkillRunTempRoot = options.SkillRunTempRoot;
            configured.SkillRunTimeoutSeconds = options.SkillRunTimeoutSeconds;
            configured.SkillRunRetentionCount = options.SkillRunRetentionCount;
            configured.AuthTestActivationCode = options.AuthTestActivationCode;
            configured.AuthLicenseValidDays = options.AuthLicenseValidDays;
            configured.AuthMaxDevices = options.AuthMaxDevices;
        });
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IAuthTokenService, LocalAuthTokenService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuthLicensingAdapter, DeterministicAuthLicensingAdapter>();
        services.AddSingleton<IProviderSettingsRepository, FileProviderSettingsRepository>();
        services.AddSingleton<IProviderSecretStore, FileProviderSecretStore>();
        services.AddSingleton<IProviderConnectivityTester, OpenAiCompatibleProviderConnectivityTester>();
        services.AddSingleton<IProjectAssetFileStore, LocalProjectAssetFileStore>();
        services.AddSingleton<IProjectAssetUploadSessionStore, LocalProjectAssetUploadSessionStore>();
        services.AddSingleton<IAssetTechnicalAnalyzer, FfmpegAssetTechnicalAnalyzer>();
        services.AddScoped<IProductionSkillRunner, PythonProductionSkillRunner>();

        if (string.Equals(options.RepositoryProvider, RepositoryProviderNames.Sqlite, StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration.GetConnectionString("MiLuStudioControlPlane");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = BuildDefaultSqliteConnectionString(options);
            }

            EnsureSqliteDirectory(connectionString);
            services.AddDbContext<MiLuStudioDbContext>(db => db.UseSqlite(connectionString));
            services.AddScoped<SqliteControlPlaneRepository>();
            services.AddScoped<IProjectRepository>(provider => provider.GetRequiredService<SqliteControlPlaneRepository>());
            services.AddScoped<IProductionJobRepository>(provider => provider.GetRequiredService<SqliteControlPlaneRepository>());
            services.AddScoped<IAssetRepository>(provider => provider.GetRequiredService<SqliteControlPlaneRepository>());
            services.AddScoped<ICostLedgerRepository>(provider => provider.GetRequiredService<SqliteControlPlaneRepository>());
            services.AddScoped<IAuthRepository, SqliteAuthRepository>();
            services.AddScoped<IControlPlaneMigrationService, SqliteMigrationService>();
            services.AddScoped<IControlPlanePreflightService, SqliteControlPlanePreflightService>();
        }
        else if (string.Equals(options.RepositoryProvider, RepositoryProviderNames.InMemory, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<InMemoryControlPlaneStore>();
            services.AddSingleton<InMemoryAuthRepository>();
            services.AddSingleton<IProjectRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
            services.AddSingleton<IProductionJobRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
            services.AddSingleton<IAssetRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
            services.AddSingleton<ICostLedgerRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
            services.AddSingleton<IAuthRepository>(provider => provider.GetRequiredService<InMemoryAuthRepository>());
            services.AddSingleton<InMemoryControlPlanePreflightService>();
            services.AddSingleton<IControlPlanePreflightService>(provider => provider.GetRequiredService<InMemoryControlPlanePreflightService>());
            services.AddSingleton<IControlPlaneMigrationService>(provider => provider.GetRequiredService<InMemoryControlPlanePreflightService>());
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported ControlPlane:RepositoryProvider '{options.RepositoryProvider}'. Use SQLite by default or explicitly set InMemory for smoke tests.");
        }

        return services;
    }

    private static ControlPlaneOptions LoadOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(ControlPlaneOptions.SectionName);
        return new ControlPlaneOptions
        {
            RepositoryProvider = section["RepositoryProvider"] ?? RepositoryProviderNames.Sqlite,
            MigrationsPath = section["MigrationsPath"] ?? "backend/control-plane/db/sqlite",
            StorageRoot = section["StorageRoot"] ?? "D:\\code\\MiLuStudio\\storage",
            UploadsRoot = section["UploadsRoot"] ?? "D:\\code\\MiLuStudio\\uploads",
            FfmpegBinPath = section["FfmpegBinPath"] ?? "D:\\code\\MiLuStudio\\runtime\\ffmpeg\\bin",
            OcrTesseractPath = section["OcrTesseractPath"] ?? string.Empty,
            OcrTessdataPath = section["OcrTessdataPath"] ?? string.Empty,
            OcrLanguages = section["OcrLanguages"] ?? "chi_sim+eng;eng",
            AssetParseTimeoutSeconds = int.TryParse(section["AssetParseTimeoutSeconds"], out var assetParseTimeoutSeconds)
                ? Math.Clamp(assetParseTimeoutSeconds, 5, 300)
                : 60,
            AssetTranscodeTimeoutSeconds = int.TryParse(section["AssetTranscodeTimeoutSeconds"], out var assetTranscodeTimeoutSeconds)
                ? Math.Clamp(assetTranscodeTimeoutSeconds, 30, 600)
                : 180,
            AssetVideoFrameLimit = int.TryParse(section["AssetVideoFrameLimit"], out var assetVideoFrameLimit)
                ? Math.Clamp(assetVideoFrameLimit, 1, 8)
                : 8,
            OcrTimeoutSeconds = int.TryParse(section["OcrTimeoutSeconds"], out var ocrTimeoutSeconds)
                ? Math.Clamp(ocrTimeoutSeconds, 5, 180)
                : 45,
            ProviderSettingsPath = section["ProviderSettingsPath"] ?? string.Empty,
            ProviderSecretStorePath = section["ProviderSecretStorePath"] ?? string.Empty,
            WorkerId = section["WorkerId"] ?? Environment.MachineName,
            PythonExecutablePath = section["PythonExecutablePath"] ?? "D:\\soft\\program\\Python\\Python313\\python.exe",
            PythonSkillsRoot = section["PythonSkillsRoot"] ?? "D:\\code\\MiLuStudio\\backend\\sidecars\\python-skills",
            SkillRunTempRoot = section["SkillRunTempRoot"] ?? "D:\\code\\MiLuStudio\\.tmp\\skill-runs",
            SkillRunTimeoutSeconds = int.TryParse(section["SkillRunTimeoutSeconds"], out var timeoutSeconds)
                ? timeoutSeconds
                : 120,
            SkillRunRetentionCount = int.TryParse(section["SkillRunRetentionCount"], out var retentionCount)
                ? Math.Max(1, retentionCount)
                : 30,
            AuthTestActivationCode = section["AuthTestActivationCode"] ?? "MILU-STAGE16-TEST",
            AuthLicenseValidDays = int.TryParse(section["AuthLicenseValidDays"], out var licenseDays)
                ? Math.Max(1, licenseDays)
                : 30,
            AuthMaxDevices = int.TryParse(section["AuthMaxDevices"], out var maxDevices)
                ? Math.Max(1, maxDevices)
                : 2
        };
    }

    private static string BuildDefaultSqliteConnectionString(ControlPlaneOptions options)
    {
        var databasePath = Path.Combine(options.StorageRoot, "milu-control-plane.sqlite3");
        return $"Data Source={databasePath}";
    }

    private static void EnsureSqliteDirectory(string connectionString)
    {
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource) ||
            string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(builder.DataSource));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
