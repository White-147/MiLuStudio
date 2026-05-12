namespace MiLuStudio.Application.Production;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class ProductionJobService
{
    private readonly IClock _clock;
    private readonly IProductionJobRepository _jobs;
    private readonly IProjectRepository _projects;

    public ProductionJobService(IClock clock, IProductionJobRepository jobs, IProjectRepository projects)
    {
        _clock = clock;
        _jobs = jobs;
        _projects = projects;
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

    public async Task<ProductionJobDto?> StartAsync(string projectId, StartProductionJobRequest request, CancellationToken cancellationToken)
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
            CurrentStage = ProductionStageCatalog.All[0].Id,
            Status = ProductionJobStatus.Running,
            ProgressPercent = 0,
            StartedAt = now
        };

        var tasks = ProductionStageCatalog.All
            .Select((stage, index) => new GenerationTask
            {
                Id = $"task_{Guid.NewGuid():N}",
                JobId = job.Id,
                ProjectId = projectId,
                SkillName = stage.Skill,
                Provider = "mock-control-plane",
                InputJson = $$"""{"stage":"{{stage.Id}}","requestedBy":"{{request.RequestedBy ?? "ui"}}"}""",
                Status = index == 0 ? GenerationTaskStatus.Running : GenerationTaskStatus.Waiting,
                AttemptCount = 0,
                CostEstimate = index < 3 ? 0.01m * (index + 1) : 0
            })
            .ToList();

        await _jobs.AddAsync(job, tasks, cancellationToken);

        return ToDto(job, tasks);
    }

    public async Task<ProductionJobDto?> PauseAsync(string jobId, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetAsync(jobId, cancellationToken);

        if (job is null)
        {
            return null;
        }

        job.Status = ProductionJobStatus.Paused;
        await _jobs.UpdateAsync(job, cancellationToken);

        var tasks = await _jobs.ListTasksAsync(jobId, cancellationToken);
        return ToDto(job, tasks);
    }

    public async Task<ProductionJobDto?> ResumeAsync(string jobId, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetAsync(jobId, cancellationToken);

        if (job is null)
        {
            return null;
        }

        job.Status = ProductionJobStatus.Running;
        await _jobs.UpdateAsync(job, cancellationToken);

        var tasks = await _jobs.ListTasksAsync(jobId, cancellationToken);
        return ToDto(job, tasks);
    }

    public async Task<ProductionJobDto?> RetryAsync(string jobId, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetAsync(jobId, cancellationToken);

        if (job is null)
        {
            return null;
        }

        job.Status = ProductionJobStatus.Running;
        job.ErrorMessage = null;
        await _jobs.UpdateAsync(job, cancellationToken);

        var tasks = await _jobs.ListTasksAsync(jobId, cancellationToken);
        foreach (var task in tasks.Where(task => task.Status == GenerationTaskStatus.Failed))
        {
            task.Status = GenerationTaskStatus.Waiting;
            task.ErrorMessage = null;
        }

        await _jobs.ReplaceTasksAsync(jobId, tasks, cancellationToken);

        return ToDto(job, tasks);
    }

    public async IAsyncEnumerable<ProductionJobEventDto> StreamMockEventsAsync(
        string jobId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var job = await _jobs.GetAsync(jobId, cancellationToken);

        if (job is null)
        {
            yield break;
        }

        var tasks = (await _jobs.ListTasksAsync(jobId, cancellationToken)).ToList();

        for (var index = 0; index < ProductionStageCatalog.All.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var stage = ProductionStageCatalog.All[index];
            var progress = (int)Math.Round((index + 1) * 100m / ProductionStageCatalog.All.Count);
            var status = stage.NeedsReview ? GenerationTaskStatus.Review : GenerationTaskStatus.Running;

            job.CurrentStage = stage.Id;
            job.ProgressPercent = progress;
            job.Status = ProductionJobStatus.Running;
            SetTaskStatus(tasks, stage.Skill, status);

            await _jobs.UpdateAsync(job, cancellationToken);
            await _jobs.ReplaceTasksAsync(jobId, tasks, cancellationToken);

            yield return new ProductionJobEventDto(
                TypeFor(index, stage),
                job.Id,
                job.ProjectId,
                stage.Id,
                stage.Label,
                stage.Skill,
                StageStatus(status),
                progress,
                stage.NeedsReview ? $"{stage.Label} 已生成，等待用户确认。" : $"{stage.Label} 正在执行。",
                _clock.Now);

            await Task.Delay(TimeSpan.FromMilliseconds(650), cancellationToken);

            SetTaskStatus(tasks, stage.Skill, GenerationTaskStatus.Completed);
        }

        job.ProgressPercent = 100;
        job.CurrentStage = "completed";
        job.Status = ProductionJobStatus.Completed;
        job.FinishedAt = _clock.Now;
        await _jobs.UpdateAsync(job, cancellationToken);
        await _jobs.ReplaceTasksAsync(jobId, tasks, cancellationToken);

        yield return new ProductionJobEventDto(
            "artifact_ready",
            job.Id,
            job.ProjectId,
            "delivery",
            "导出包",
            "export_packager",
            "done",
            100,
            "mock 生产任务已完成，后续阶段会接入真实 Worker 和资产索引。",
            _clock.Now);
    }

    private static ProductionJobDto ToDto(ProductionJob job, IReadOnlyList<GenerationTask> tasks)
    {
        return new ProductionJobDto(
            job.Id,
            job.ProjectId,
            FormatJobStatus(job.Status),
            job.CurrentStage,
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
            var task = tasks.FirstOrDefault(task => task.SkillName == stage.Skill);

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

    private static void SetTaskStatus(IReadOnlyList<GenerationTask> tasks, string skillName, GenerationTaskStatus status)
    {
        var task = tasks.FirstOrDefault(task => task.SkillName == skillName);

        if (task is null)
        {
            return;
        }

        task.Status = status;
        task.AttemptCount = Math.Max(1, task.AttemptCount);
    }

    private static string TypeFor(int index, ProductionStageDefinition stage)
    {
        if (index == 0)
        {
            return "task_started";
        }

        return stage.NeedsReview ? "checkpoint_required" : "stage_changed";
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
}
