namespace MiLuStudio.Worker;

using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Production;
using MiLuStudio.Infrastructure.Configuration;

public sealed class ProductionWorker : BackgroundService
{
    private static readonly TimeSpan ClaimLeaseDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PollDelay = TimeSpan.FromSeconds(3);

    private readonly IClock _clock;
    private readonly ILogger<ProductionWorker> _logger;
    private readonly ControlPlaneOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductionWorker(
        ILogger<ProductionWorker> logger,
        IServiceScopeFactory scopeFactory,
        IClock clock,
        IOptions<ControlPlaneOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _clock = clock;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "MiLuStudio Worker ready. RepositoryProvider={RepositoryProvider}; durable task claiming stays behind the repository boundary.",
            _options.RepositoryProvider);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var jobs = scope.ServiceProvider.GetRequiredService<IProductionJobRepository>();
            var executor = scope.ServiceProvider.GetRequiredService<ProductionSkillExecutionService>();
            var claimed = await jobs.ClaimNextTaskAsync(
                _options.WorkerId,
                _clock.Now,
                ClaimLeaseDuration,
                stoppingToken);

            if (claimed is null)
            {
                _logger.LogInformation("Worker heartbeat: no claimable task.");
            }
            else
            {
                _logger.LogInformation(
                    "Worker claimed task {TaskId} for skill {SkillName} in job {JobId}.",
                    claimed.Id,
                    claimed.SkillName,
                    claimed.JobId);

                var result = await executor.ExecuteAsync(claimed, stoppingToken);
                _logger.LogInformation(
                    "Worker finished task {TaskId} for skill {SkillName} with status {Status}: {Message}",
                    result.TaskId,
                    result.SkillName,
                    result.Status,
                    result.Message);
                continue;
            }

            await Task.Delay(PollDelay, stoppingToken);
        }
    }
}
