using System.Text.Json;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Auth;
using MiLuStudio.Application.Production;
using MiLuStudio.Application.Projects;
using MiLuStudio.Infrastructure;
using MiLuStudio.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);
var allowedDesktopOrigin = builder.Configuration["ControlPlane:AllowedDesktopOrigin"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("MiLuStudioLocalDev", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => IsAllowedLocalOrigin(origin, allowedDesktopOrigin))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<AuthLicensingService>();
builder.Services.AddScoped<ProductionJobService>();
builder.Services.AddScoped<ProductionSkillExecutionService>();
builder.Services.AddScoped<SkillEnvelopePersistenceService>();
builder.Services.AddScoped<StoryboardEditingService>();
builder.Services.AddScoped<TaskQueueService>();
builder.Services.AddMiLuStudioInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCors("MiLuStudioLocalDev");

app.Use(async (context, next) =>
{
    var desktopMode = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<bool>("ControlPlane:DesktopMode");
    var desktopSessionToken = context.RequestServices.GetRequiredService<IConfiguration>()["ControlPlane:DesktopSessionToken"];

    if (desktopMode &&
        HttpMethods.IsPost(context.Request.Method) &&
        string.Equals(context.Request.Path.Value, "/api/system/migrations/apply", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Desktop runtime does not execute database migrations. Run migrations before launching the desktop host."
        });
        return;
    }

    if (!string.IsNullOrWhiteSpace(desktopSessionToken) && IsUnsafeHttpMethod(context.Request.Method))
    {
        var providedToken = context.Request.Headers["X-MiLuStudio-Desktop-Token"].ToString();
        if (!string.Equals(providedToken, desktopSessionToken, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Desktop session token is missing or invalid." });
            return;
        }
    }

    await next();
});

app.Use(async (context, next) =>
{
    var requirement = GetAuthRequirement(context.Request.Path);
    if (requirement == AuthRequirement.None)
    {
        await next();
        return;
    }

    var auth = context.RequestServices.GetRequiredService<AuthLicensingService>();
    var validation = await auth.ValidateRequestAsync(
        ResolveAccessToken(context.Request),
        false,
        context.RequestAborted);

    if (!validation.Allowed)
    {
        context.Response.StatusCode = validation.StatusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            error = validation.Message,
            code = validation.Code
        });
        return;
    }

    context.Items["MiLuStudio.Auth"] = validation.Principal;
    await next();
});

app.MapGet("/", () => Results.Redirect("/health"));

app.MapGet("/health", (IConfiguration configuration) => Results.Ok(new
{
    service = "MiLuStudio Control API",
    status = "ok",
    mode = "stage-16-auth-session",
    defaultApiBaseUrl = "http://127.0.0.1:5368",
    repositoryProvider = configuration["ControlPlane:RepositoryProvider"] ?? RepositoryProviderNames.PostgreSql
}));

app.MapPost("/api/auth/register", async Task<IResult> (
    RegisterAccountRequest request,
    AuthLicensingService auth,
    CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await auth.RegisterAsync(request, cancellationToken));
    }
    catch (AuthCommandException error)
    {
        return AuthError(error);
    }
});

app.MapPost("/api/auth/login", async Task<IResult> (
    LoginRequest request,
    AuthLicensingService auth,
    CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await auth.LoginAsync(request, cancellationToken));
    }
    catch (AuthCommandException error)
    {
        return AuthError(error);
    }
});

app.MapPost("/api/auth/refresh", async Task<IResult> (
    RefreshSessionRequest request,
    AuthLicensingService auth,
    CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await auth.RefreshAsync(request, cancellationToken));
    }
    catch (AuthCommandException error)
    {
        return AuthError(error);
    }
});

app.MapPost("/api/auth/logout", async Task<IResult> (
    LogoutRequest request,
    AuthLicensingService auth,
    HttpContext context) =>
{
    await auth.LogoutAsync(ResolveAccessToken(context.Request), request, context.RequestAborted);
    return Results.Ok(new { status = "signed_out" });
});

app.MapGet("/api/auth/me", async (
    AuthLicensingService auth,
    HttpContext context) =>
{
    return Results.Ok(await auth.GetStateAsync(ResolveAccessToken(context.Request), context.RequestAborted));
});

app.MapPost("/api/auth/devices/bind", async Task<IResult> (
    BindDeviceRequest request,
    AuthLicensingService auth,
    HttpContext context) =>
{
    try
    {
        return Results.Ok(await auth.BindDeviceAsync(ResolveAccessToken(context.Request), request, context.RequestAborted));
    }
    catch (AuthCommandException error)
    {
        return AuthError(error);
    }
});

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

app.MapPost("/api/projects", async Task<IResult> (
    CreateProjectRequest request,
    ProjectService projects,
    CancellationToken cancellationToken) =>
{
    try
    {
        var created = await projects.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/projects/{created.Id}", created);
    }
    catch (ProjectValidationException error)
    {
        return Results.BadRequest(new { error = error.Message, details = error.Details });
    }
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

app.MapPatch("/api/projects/{projectId}", async Task<IResult> (
    string projectId,
    UpdateProjectRequest request,
    ProjectService projects,
    CancellationToken cancellationToken) =>
{
    try
    {
        var project = await projects.UpdateAsync(projectId, request, cancellationToken);
        return project is null ? Results.NotFound() : Results.Ok(project);
    }
    catch (ProjectValidationException error)
    {
        return Results.BadRequest(new { error = error.Message, details = error.Details });
    }
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
        task.CheckpointNotes,
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

app.MapPost("/api/production-jobs/{jobId}/checkpoint", async Task<IResult> (
    string jobId,
    ProductionCheckpointRequest request,
    ProductionJobService jobs,
    CancellationToken cancellationToken) =>
{
    try
    {
        var job = await jobs.CheckpointAsync(jobId, request, cancellationToken);
        return job is null ? Results.NotFound() : Results.Ok(job);
    }
    catch (ProductionCommandValidationException error)
    {
        return Results.BadRequest(new { error = error.Message });
    }
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

app.MapPatch("/api/generation-tasks/{taskId}/storyboard", async Task<IResult> (
    string taskId,
    StoryboardEditRequest request,
    StoryboardEditingService storyboards,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await storyboards.SaveAsync(taskId, request, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (StoryboardEditValidationException error)
    {
        return Results.BadRequest(new { error = error.Message });
    }
    catch (JsonException error)
    {
        return Results.BadRequest(new { error = $"Storyboard output is not valid JSON: {error.Message}" });
    }
});

app.MapPost("/api/generation-tasks/{taskId}/storyboard/shots/{shotId}/regenerate", async Task<IResult> (
    string taskId,
    string shotId,
    StoryboardShotRegenerateRequest request,
    StoryboardEditingService storyboards,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await storyboards.RegenerateShotAsync(taskId, shotId, request, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (StoryboardEditValidationException error)
    {
        return Results.BadRequest(new { error = error.Message });
    }
    catch (JsonException error)
    {
        return Results.BadRequest(new { error = $"Storyboard output is not valid JSON: {error.Message}" });
    }
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

static bool IsAllowedLocalOrigin(string origin, string? allowedDesktopOrigin)
{
    if (!string.IsNullOrWhiteSpace(allowedDesktopOrigin))
    {
        return string.Equals(
            origin.TrimEnd('/'),
            allowedDesktopOrigin.TrimEnd('/'),
            StringComparison.OrdinalIgnoreCase);
    }

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
        (uri.IsLoopback ||
            string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase));
}

static bool IsUnsafeHttpMethod(string method) =>
    HttpMethods.IsPost(method) ||
    HttpMethods.IsPut(method) ||
    HttpMethods.IsPatch(method) ||
    HttpMethods.IsDelete(method);

static string? ResolveAccessToken(HttpRequest request)
{
    var authorization = request.Headers.Authorization.ToString();
    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        return authorization["Bearer ".Length..].Trim();
    }

    return request.Query.TryGetValue("access_token", out var accessToken) ? accessToken.ToString() : null;
}

static IResult AuthError(AuthCommandException error)
{
    return Results.Json(
        new { error = error.Message, code = error.Code },
        statusCode: error.StatusCode);
}

static AuthRequirement GetAuthRequirement(PathString path)
{
    var value = path.Value ?? string.Empty;
    if (value.StartsWith("/api/projects", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("/api/production-jobs", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("/api/generation-tasks", StringComparison.OrdinalIgnoreCase))
    {
        return AuthRequirement.Authenticated;
    }

    return AuthRequirement.None;
}

internal enum AuthRequirement
{
    None,
    Authenticated
}
