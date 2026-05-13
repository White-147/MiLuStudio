namespace MiLuStudio.Worker;

using MiLuStudio.Application.Projects;

public sealed class ProductionWorker : BackgroundService
{
    private readonly ILogger<ProductionWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductionWorker(ILogger<ProductionWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MiLuStudio Worker ready. Stage 3 uses control-plane state transitions with in-memory runtime storage; durable queue claiming lands in a later adapter stage.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var projects = scope.ServiceProvider.GetRequiredService<ProjectService>();
            var summaries = await projects.ListAsync(stoppingToken);

            _logger.LogInformation("Worker heartbeat: {ProjectCount} projects visible to the control plane.", summaries.Count);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
