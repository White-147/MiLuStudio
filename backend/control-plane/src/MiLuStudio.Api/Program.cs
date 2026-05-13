using System.Text.Json;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Production;
using MiLuStudio.Application.Projects;
using MiLuStudio.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("MiLuStudioLocalDev", policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5173", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<ProductionJobService>();
builder.Services.AddScoped<ProductionSkillExecutionService>();
builder.Services.AddScoped<SkillEnvelopePersistenceService>();
builder.Services.AddScoped<TaskQueueService>();
builder.Services.AddMiLuStudioInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCors("MiLuStudioLocalDev");

app.MapGet("/", () => Results.Redirect("/health"));

app.MapGet("/health", (IConfiguration configuration) => Results.Ok(new
{
    service = "MiLuStudio Control API",
    status = "ok",
    mode = "stage-13-postgresql-worker-skills",
    repositoryProvider = configuration["ControlPlane:RepositoryProvider"] ?? "InMemory"
}));

app.MapGet("/api/system/preflight", async (
    IControlPlanePreflightService preflight,
    CancellationToken cancellationToken) =>
{
    var report = await preflight.CheckAsync(cancellationToken);
    return report.Healthy ? Results.Ok(report) : Results.Problem(
        title: "MiLuStudio preflight failed",
        detail: "Backend configuration requires attention before using the selected repository provider.",
        statusCode: StatusCodes.Status503ServiceUnavailable,
        extensions: new Dictionary<string, object?> { ["preflight"] = report });
});

app.MapGet("/api/system/migrations", async (
    IControlPlaneMigrationService migrations,
    CancellationToken cancellationToken) =>
{
    var status = await migrations.GetStatusAsync(cancellationToken);
    return Results.Ok(status);
});

app.MapPost("/api/system/migrations/apply", async (
    IControlPlaneMigrationService migrations,
    CancellationToken cancellationToken) =>
{
    var result = await migrations.ApplyPendingAsync(cancellationToken);
    return Results.Ok(result);
});

app.MapGet("/api/projects", async (ProjectService projects, CancellationToken cancellationToken) =>
{
    var results = await projects.ListAsync(cancellationToken);
    return Results.Ok(results);
});

app.MapPost("/api/projects", async (
    CreateProjectRequest request,
    ProjectService projects,
    CancellationToken cancellationToken) =>
{
    var created = await projects.CreateAsync(request, cancellationToken);
    return Results.Created($"/api/projects/{created.Id}", created);
});

app.MapGet("/api/projects/{projectId}", async (
    string projectId,
    ProjectService projects,
    CancellationToken cancellationToken) =>
{
    var project = await projects.GetAsync(projectId, cancellationToken);
    return project is null ? Results.NotFound() : Results.Ok(project);
});

app.MapGet("/api/projects/{projectId}/assets", async (
    string projectId,
    IAssetRepository assets,
    CancellationToken cancellationToken) =>
{
    var results = await assets.ListAssetsByProjectAsync(projectId, cancellationToken);
    return Results.Ok(results);
});

app.MapGet("/api/projects/{projectId}/cost-ledger", async (
    string projectId,
    ICostLedgerRepository costLedger,
    CancellationToken cancellationToken) =>
{
    var results = await costLedger.ListCostByProjectAsync(projectId, cancellationToken);
    return Results.Ok(results);
});

app.MapPatch("/api/projects/{projectId}", async (
    string projectId,
    UpdateProjectRequest request,
    ProjectService projects,
    CancellationToken cancellationToken) =>
{
    var project = await projects.UpdateAsync(projectId, request, cancellationToken);
    return project is null ? Results.NotFound() : Results.Ok(project);
});

app.MapPost("/api/projects/{projectId}/production-jobs", async (
    string projectId,
    StartProductionJobRequest request,
    ProductionJobService jobs,
    CancellationToken cancellationToken) =>
{
    var job = await jobs.StartAsync(projectId, request, cancellationToken);
    return job is null ? Results.NotFound() : Results.Created($"/api/production-jobs/{job.Id}", job);
});

app.MapGet("/api/production-jobs/{jobId}", async (
    string jobId,
    ProductionJobService jobs,
    CancellationToken cancellationToken) =>
{
    var job = await jobs.GetAsync(jobId, cancellationToken);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

app.MapGet("/api/production-jobs/{jobId}/tasks", async (
    string jobId,
    IProductionJobRepository jobs,
    CancellationToken cancellationToken) =>
{
    var job = await jobs.GetAsync(jobId, cancellationToken);
    if (job is null)
    {
        return Results.NotFound();
    }

    var tasks = await jobs.ListTasksAsync(jobId, cancellationToken);
    return Results.Ok(tasks.Select(task => new
    {
        task.Id,
        task.JobId,
        task.ProjectId,
        task.ShotId,
        task.QueueIndex,
        task.SkillName,
        task.Provider,
        task.InputJson,
        task.OutputJson,
        Status = task.Status switch
        {
            MiLuStudio.Domain.GenerationTaskStatus.Running => "running",
            MiLuStudio.Domain.GenerationTaskStatus.Review => "review",
            MiLuStudio.Domain.GenerationTaskStatus.Completed => "completed",
            MiLuStudio.Domain.GenerationTaskStatus.Failed => "failed",
            _ => "waiting"
        },
        task.AttemptCount,
        task.CostEstimate,
        task.CostActual,
        task.StartedAt,
        task.FinishedAt,
        task.LockedBy,
        task.LockedUntil,
        task.LastHeartbeatAt,
        task.ErrorMessage
    }));
});

app.MapPost("/api/production-jobs/{jobId}/pause", async (
    string jobId,
    ProductionJobService jobs,
    CancellationToken cancellationToken) =>
{
    var job = await jobs.PauseAsync(jobId, cancellationToken);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

app.MapPost("/api/production-jobs/{jobId}/resume", async (
    string jobId,
    ProductionJobService jobs,
    CancellationToken cancellationToken) =>
{
    var job = await jobs.ResumeAsync(jobId, cancellationToken);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

app.MapPost("/api/production-jobs/{jobId}/retry", async (
    string jobId,
    ProductionJobService jobs,
    CancellationToken cancellationToken) =>
{
    var job = await jobs.RetryAsync(jobId, cancellationToken);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

app.MapPost("/api/production-jobs/{jobId}/checkpoint", async (
    string jobId,
    ProductionCheckpointRequest request,
    ProductionJobService jobs,
    CancellationToken cancellationToken) =>
{
    var job = await jobs.CheckpointAsync(jobId, request, cancellationToken);
    return job is null ? Results.NotFound() : Results.Ok(job);
});

app.MapPost("/api/generation-tasks/{taskId}/output", async (
    string taskId,
    PersistSkillEnvelopeRequest request,
    SkillEnvelopePersistenceService persistence,
    CancellationToken cancellationToken) =>
{
    var result = await persistence.PersistAsync(taskId, request, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapGet("/api/production-jobs/{jobId}/events", async (
    string jobId,
    ProductionJobService jobs,
    HttpContext context) =>
{
    var cancellationToken = context.RequestAborted;
    var job = await jobs.GetAsync(jobId, cancellationToken);

    if (job is null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.Headers.CacheControl = "no-cache";
    context.Response.Headers.Connection = "keep-alive";
    context.Response.Headers.Append("X-Accel-Buffering", "no");
    context.Response.ContentType = "text/event-stream";

    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    await foreach (var productionEvent in jobs.StreamEventsAsync(jobId, cancellationToken))
    {
        var payload = JsonSerializer.Serialize(productionEvent, jsonOptions);
        await context.Response.WriteAsync($"event: {productionEvent.Type}\n", cancellationToken);
        await context.Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }
});

app.Run();
