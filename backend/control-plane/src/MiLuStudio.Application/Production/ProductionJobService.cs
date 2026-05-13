namespace MiLuStudio.Application.Production;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class ProductionJobService
{
    private static readonly TimeSpan MockEventDelay = TimeSpan.FromMilliseconds(650);

    private readonly IClock _clock;
    private readonly IProductionJobRepository _jobs;
    private readonly IProjectRepository _projects;
    private readonly ProductionStateMachine _stateMachine;
    private readonly TaskQueueService _taskQueue;

    public ProductionJobService(
        IClock clock,
        IProductionJobRepository jobs,
        IProjectRepository projects,
        TaskQueueService taskQueue)
    {
        _clock = clock;
        _jobs = jobs;
        _projects = projects;
        _taskQueue = taskQueue;
        _stateMachine = new ProductionStateMachine(taskQueue);
    }

    public async Task<ProductionJobDto?> GetAsync(string jobId, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetAsync(jobId, cancellationToken);

        if (job is null)
        {
            return null;
        }

        var tasks = await _jobs.ListTasksAsync(jobId, cancellationToken);

        return ToDto(job, tasks);
    }

    public async Task<ProductionJobDto?> StartAsync(
        string projectId,
        StartProductionJobRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(projectId, cancellationToken);

        if (project is null)
        {
            return null;
        }

        var now = _clock.Now;
        project.Status = ProjectStatus.Running;
        project.UpdatedAt = now;
        await _projects.UpdateAsync(project, cancellationToken);

        var job = new ProductionJob
        {
            Id = $"job_{Guid.NewGuid():N}",
            ProjectId = projectId,
            CurrentStage = ProductionStage.Created,
            Status = ProductionJobStatus.Running,
            ProgressPercent = 0,
            StartedAt = now
        };

        var tasks = _taskQueue.CreateInitialTasks(job.Id, projectId, request.RequestedBy);

        await _jobs.AddAsync(job, tasks, cancellationToken);

        return ToDto(job, tasks);
    }

    public async Task<ProductionJobDto?> PauseAsync(string jobId, CancellationToken cancellationToken)
    {
        var snapshot = await LoadSnapshotAsync(jobId, cancellationToken);

        if (snapshot is null)
        {
            return null;
        }

        _stateMachine.Pause(snapshot.Job);
        await PersistAsync(snapshot, cancellationToken);

        return ToDto(snapshot.Job, snapshot.Tasks);
    }

    public async Task<ProductionJobDto?> ResumeAsync(string jobId, CancellationToken cancellationToken)
    {
        var snapshot = await LoadSnapshotAsync(jobId, cancellationToken);

        if (snapshot is null)
        {
            return null;
        }

        _stateMachine.Resume(snapshot.Job, snapshot.Tasks);
        await PersistAsync(snapshot, cancellationToken);

        return ToDto(snapshot.Job, snapshot.Tasks);
    }

    public async Task<ProductionJobDto?> RetryAsync(string jobId, CancellationToken cancellationToken)
    {
        var snapshot = await LoadSnapshotAsync(jobId, cancellationToken);

        if (snapshot is null)
        {
            return null;
        }

        _stateMachine.PrepareRetry(snapshot.Job, snapshot.Tasks);
        await PersistAsync(snapshot, cancellationToken);

        return ToDto(snapshot.Job, snapshot.Tasks);
    }

    public async Task<ProductionJobDto?> CheckpointAsync(
        string jobId,
        ProductionCheckpointRequest request,
        CancellationToken cancellationToken)
    {
        var snapshot = await LoadSnapshotAsync(jobId, cancellationToken);

        if (snapshot is null)
        {
            return null;
        }

        _stateMachine.ApplyCheckpoint(
            snapshot.Job,
            snapshot.Tasks,
            request.Approved ?? true,
            _clock.Now,
            request.Notes);

        await PersistAsync(snapshot, cancellationToken);

        return ToDto(snapshot.Job, snapshot.Tasks);
    }

    public async IAsyncEnumerable<ProductionJobEventDto> StreamMockEventsAsync(
        string jobId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var snapshot = await LoadSnapshotAsync(jobId, cancellationToken);

            if (snapshot is null)
            {
                yield break;
            }

            var transition = _stateMachine.Advance(snapshot.Job, snapshot.Tasks, _clock.Now);

            await PersistAsync(snapshot, cancellationToken);

            yield return ToEventDto(snapshot.Job, transition);

            if (transition.IsTerminal)
            {
                yield break;
            }

            await Task.Delay(MockEventDelay, cancellationToken);
        }
    }

    private async Task<ProductionJobSnapshot?> LoadSnapshotAsync(string jobId, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetAsync(jobId, cancellationToken);

        if (job is null)
        {
            return null;
        }

        var tasks = (await _jobs.ListTasksAsync(jobId, cancellationToken)).ToList();
        return new ProductionJobSnapshot(job, tasks);
    }

    private async Task PersistAsync(ProductionJobSnapshot snapshot, CancellationToken cancellationToken)
    {
        await _jobs.UpdateAsync(snapshot.Job, cancellationToken);
        await _jobs.ReplaceTasksAsync(snapshot.Job.Id, snapshot.Tasks, cancellationToken);
    }

    private static ProductionJobDto ToDto(ProductionJob job, IReadOnlyList<GenerationTask> tasks)
    {
        return new ProductionJobDto(
            job.Id,
            job.ProjectId,
            FormatJobStatus(job.Status),
            ProductionStageCatalog.ExternalIdFor(job.CurrentStage),
            job.ProgressPercent,
            FormatDate(job.StartedAt),
            job.FinishedAt is null ? null : FormatDate(job.FinishedAt.Value),
            job.ErrorMessage,
            ToStageDtos(tasks));
    }

    private static IReadOnlyList<ProductionStageDto> ToStageDtos(IReadOnlyList<GenerationTask> tasks)
    {
        return ProductionStageCatalog.All.Select(stage =>
        {
            var task = tasks.FirstOrDefault(task => string.Equals(task.SkillName, stage.Skill, StringComparison.OrdinalIgnoreCase));

            return new ProductionStageDto(
                stage.Id,
                stage.Label,
                stage.Skill,
                task is null ? "waiting" : StageStatus(task.Status),
                stage.Duration,
                stage.Cost,
                stage.NeedsReview);
        }).ToList();
    }

    private ProductionJobEventDto ToEventDto(ProductionJob job, ProductionStateTransition transition)
    {
        return new ProductionJobEventDto(
            transition.EventType,
            job.Id,
            job.ProjectId,
            transition.StageId,
            transition.StageLabel,
            transition.Skill,
            StageStatus(transition.TaskStatus),
            FormatJobStatus(job.Status),
            transition.Progress,
            transition.Message,
            _clock.Now);
    }

    private static string FormatJobStatus(ProductionJobStatus status)
    {
        return status switch
        {
            ProductionJobStatus.Queued => "queued",
            ProductionJobStatus.Running => "running",
            ProductionJobStatus.Paused => "paused",
            ProductionJobStatus.Completed => "completed",
            ProductionJobStatus.Failed => "failed",
            _ => "queued"
        };
    }

    private static string StageStatus(GenerationTaskStatus status)
    {
        return status switch
        {
            GenerationTaskStatus.Completed => "done",
            GenerationTaskStatus.Running => "running",
            GenerationTaskStatus.Review => "review",
            GenerationTaskStatus.Failed => "blocked",
            _ => "waiting"
        };
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    private sealed record ProductionJobSnapshot(ProductionJob Job, IReadOnlyList<GenerationTask> Tasks);
}
