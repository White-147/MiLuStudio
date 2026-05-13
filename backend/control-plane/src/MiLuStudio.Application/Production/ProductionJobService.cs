namespace MiLuStudio.Application.Production;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class ProductionJobService
{
    private static readonly TimeSpan EventDelay = TimeSpan.FromSeconds(1);

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

        var activeJob = await FindActiveJobAsync(projectId, cancellationToken);
        if (activeJob is not null)
        {
            var activeTasks = await _jobs.ListTasksAsync(activeJob.Id, cancellationToken);
            return ToDto(activeJob, activeTasks);
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
            request.Approved ?? throw new ProductionCommandValidationException("checkpoint must explicitly set approved to true or false."),
            _clock.Now,
            request.Notes);

        await PersistAsync(snapshot, cancellationToken);

        return ToDto(snapshot.Job, snapshot.Tasks);
    }

    private async Task<ProductionJob?> FindActiveJobAsync(string projectId, CancellationToken cancellationToken)
    {
        var jobs = await _jobs.ListByProjectAsync(projectId, cancellationToken);
        return jobs
            .Where(job => job.Status is ProductionJobStatus.Running or ProductionJobStatus.Paused or ProductionJobStatus.Queued)
            .OrderByDescending(job => job.StartedAt)
            .FirstOrDefault();
    }

    public async IAsyncEnumerable<ProductionJobEventDto> StreamEventsAsync(
        string jobId,
        [global::System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var snapshot = await LoadSnapshotAsync(jobId, cancellationToken);

            if (snapshot is null)
            {
                yield break;
            }

            var transition = ToSnapshotTransition(snapshot.Job, snapshot.Tasks);

            yield return ToEventDto(snapshot.Job, transition);

            if (transition.IsTerminal)
            {
                yield break;
            }

            await Task.Delay(EventDelay, cancellationToken);
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

    private ProductionStateTransition ToSnapshotTransition(ProductionJob job, IReadOnlyList<GenerationTask> tasks)
    {
        if (job.Status == ProductionJobStatus.Completed)
        {
            return new ProductionStateTransition(
                "artifact_ready",
                "delivery",
                "导出包",
                "export_packager",
                GenerationTaskStatus.Completed,
                100,
                "生产任务已完成，导出占位结构已写入数据库。",
                IsTerminal: true);
        }

        if (job.Status == ProductionJobStatus.Failed)
        {
            var failedTask = tasks.FirstOrDefault(task => task.Status == GenerationTaskStatus.Failed);
            var failedStage = failedTask is null ? null : ProductionStageCatalog.FindBySkill(failedTask.SkillName);

            return new ProductionStateTransition(
                "task_failed",
                failedStage?.Id ?? ProductionStageCatalog.ExternalIdFor(job.CurrentStage),
                failedStage?.Label ?? "生产失败",
                failedStage?.Skill ?? failedTask?.SkillName ?? "control_plane",
                GenerationTaskStatus.Failed,
                job.ProgressPercent,
                job.ErrorMessage ?? failedTask?.ErrorMessage ?? "生产任务失败。",
                IsTerminal: true);
        }

        var stage = ProductionStageCatalog.Find(job.CurrentStage) ?? ProductionStageCatalog.First;
        var task = _taskQueue.FindTask(tasks, stage);
        var taskStatus = task?.Status ?? GenerationTaskStatus.Waiting;

        if (job.Status == ProductionJobStatus.Paused && taskStatus == GenerationTaskStatus.Review)
        {
            return new ProductionStateTransition(
                "checkpoint_required",
                stage.Id,
                stage.Label,
                stage.Skill,
                GenerationTaskStatus.Review,
                job.ProgressPercent,
                $"{stage.Label} 等待 checkpoint 确认。",
                IsTerminal: false);
        }

        var eventType = taskStatus switch
        {
            GenerationTaskStatus.Running => "task_progress",
            GenerationTaskStatus.Review => "checkpoint_required",
            GenerationTaskStatus.Completed => "task_completed",
            GenerationTaskStatus.Failed => "task_failed",
            _ => "stage_changed"
        };

        return new ProductionStateTransition(
            eventType,
            stage.Id,
            stage.Label,
            stage.Skill,
            taskStatus,
            job.ProgressPercent,
            SnapshotMessage(stage, taskStatus),
            IsTerminal: false);
    }

    private static string SnapshotMessage(ProductionStageDefinition stage, GenerationTaskStatus status)
    {
        return status switch
        {
            GenerationTaskStatus.Running => $"{stage.Label} 正在由 Worker 执行。",
            GenerationTaskStatus.Review => $"{stage.Label} 已生成，等待用户确认。",
            GenerationTaskStatus.Completed => $"{stage.Label} 已完成。",
            GenerationTaskStatus.Failed => $"{stage.Label} 执行失败。",
            _ => $"{stage.Label} 等待 Worker 领取。"
        };
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
