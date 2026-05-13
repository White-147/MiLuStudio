namespace MiLuStudio.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Infrastructure.Configuration;
using MiLuStudio.Infrastructure.Persistence.InMemory;
using MiLuStudio.Infrastructure.Persistence.PostgreSql;
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
            configured.WorkerId = options.WorkerId;
            configured.PythonExecutablePath = options.PythonExecutablePath;
            configured.PythonSkillsRoot = options.PythonSkillsRoot;
            configured.SkillRunTempRoot = options.SkillRunTempRoot;
            configured.SkillRunTimeoutSeconds = options.SkillRunTimeoutSeconds;
        });
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IProductionSkillRunner, PythonProductionSkillRunner>();

        if (string.Equals(options.RepositoryProvider, RepositoryProviderNames.PostgreSql, StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration.GetConnectionString("MiLuStudioControlPlane");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:MiLuStudioControlPlane is required when ControlPlane:RepositoryProvider=PostgreSQL.");
            }

            services.AddDbContext<MiLuStudioDbContext>(db => db.UseNpgsql(connectionString));
            services.AddScoped<PostgreSqlControlPlaneRepository>();
            services.AddScoped<IProjectRepository>(provider => provider.GetRequiredService<PostgreSqlControlPlaneRepository>());
            services.AddScoped<IProductionJobRepository>(provider => provider.GetRequiredService<PostgreSqlControlPlaneRepository>());
            services.AddScoped<IAssetRepository>(provider => provider.GetRequiredService<PostgreSqlControlPlaneRepository>());
            services.AddScoped<ICostLedgerRepository>(provider => provider.GetRequiredService<PostgreSqlControlPlaneRepository>());
            services.AddScoped<IControlPlaneMigrationService, PostgreSqlMigrationService>();
            services.AddScoped<IControlPlanePreflightService, PostgreSqlControlPlanePreflightService>();
        }
        else
        {
            services.AddSingleton<InMemoryControlPlaneStore>();
            services.AddSingleton<IProjectRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
            services.AddSingleton<IProductionJobRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
            services.AddSingleton<IAssetRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
            services.AddSingleton<ICostLedgerRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
            services.AddSingleton<InMemoryControlPlanePreflightService>();
            services.AddSingleton<IControlPlanePreflightService>(provider => provider.GetRequiredService<InMemoryControlPlanePreflightService>());
            services.AddSingleton<IControlPlaneMigrationService>(provider => provider.GetRequiredService<InMemoryControlPlanePreflightService>());
        }

        return services;
    }

    private static ControlPlaneOptions LoadOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(ControlPlaneOptions.SectionName);
        return new ControlPlaneOptions
        {
            RepositoryProvider = section["RepositoryProvider"] ?? RepositoryProviderNames.InMemory,
            MigrationsPath = section["MigrationsPath"] ?? "backend/control-plane/db/migrations",
            StorageRoot = section["StorageRoot"] ?? "D:\\code\\MiLuStudio\\storage",
            WorkerId = section["WorkerId"] ?? Environment.MachineName,
            PythonExecutablePath = section["PythonExecutablePath"] ?? "D:\\soft\\program\\Python\\Python313\\python.exe",
            PythonSkillsRoot = section["PythonSkillsRoot"] ?? "D:\\code\\MiLuStudio\\backend\\sidecars\\python-skills",
            SkillRunTempRoot = section["SkillRunTempRoot"] ?? "D:\\code\\MiLuStudio\\.tmp\\skill-runs",
            SkillRunTimeoutSeconds = int.TryParse(section["SkillRunTimeoutSeconds"], out var timeoutSeconds)
                ? timeoutSeconds
                : 120
        };
    }
}
