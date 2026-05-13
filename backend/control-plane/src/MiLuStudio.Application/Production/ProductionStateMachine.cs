namespace MiLuStudio.Application.Production;

using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

internal sealed class ProductionStateMachine
{
    private readonly TaskQueueService _taskQueue;

    internal ProductionStateMachine(TaskQueueService taskQueue)
    {
        _taskQueue = taskQueue;
    }

    internal static IReadOnlyDictionary<ProductionStage, IReadOnlySet<ProductionStage>> LegalTransitions { get; } =
        new Dictionary<ProductionStage, IReadOnlySet<ProductionStage>>
        {
            [ProductionStage.Created] = new HashSet<ProductionStage> { ProductionStage.StoryIngesting },
            [ProductionStage.StoryIngesting] = new HashSet<ProductionStage> { ProductionStage.PlotAdapted, ProductionStage.FailedRetryable },
            [ProductionStage.PlotAdapted] = new HashSet<ProductionStage> { ProductionStage.ScriptReadyForReview, ProductionStage.FailedRetryable },
            [ProductionStage.ScriptReadyForReview] = new HashSet<ProductionStage> { ProductionStage.CharacterReadyForReview, ProductionStage.FailedRetryable },
            [ProductionStage.CharacterReadyForReview] = new HashSet<ProductionStage> { ProductionStage.StyleReadyForReview, ProductionStage.FailedRetryable },
            [ProductionStage.StyleReadyForReview] = new HashSet<ProductionStage> { ProductionStage.StoryboardReadyForReview, ProductionStage.FailedRetryable },
            [ProductionStage.StoryboardReadyForReview] = new HashSet<ProductionStage> { ProductionStage.ImagePromptsReady, ProductionStage.FailedRetryable },
            [ProductionStage.ImagePromptsReady] = new HashSet<ProductionStage> { ProductionStage.ImagesReadyForReview, ProductionStage.FailedRetryable },
            [ProductionStage.ImagesReadyForReview] = new HashSet<ProductionStage> { ProductionStage.VideoPromptsReady, ProductionStage.FailedNeedsUser },
            [ProductionStage.VideoPromptsReady] = new HashSet<ProductionStage> { ProductionStage.VideosReadyForReview, ProductionStage.FailedRetryable },
            [ProductionStage.VideosReadyForReview] = new HashSet<ProductionStage> { ProductionStage.AudioReadyForReview, ProductionStage.FailedNeedsUser },
            [ProductionStage.AudioReadyForReview] = new HashSet<ProductionStage> { ProductionStage.SubtitlesReady, ProductionStage.FailedRetryable },
            [ProductionStage.SubtitlesReady] = new HashSet<ProductionStage> { ProductionStage.EditReadyForQualityCheck, ProductionStage.FailedRetryable },
            [ProductionStage.EditReadyForQualityCheck] = new HashSet<ProductionStage> { ProductionStage.QualityReadyForReview, ProductionStage.FailedRetryable },
            [ProductionStage.QualityReadyForReview] = new HashSet<ProductionStage> { ProductionStage.Exporting, ProductionStage.FailedNeedsUser },
            [ProductionStage.Exporting] = new HashSet<ProductionStage> { ProductionStage.Completed, ProductionStage.FailedRetryable },
            [ProductionStage.FailedRetryable] = new HashSet<ProductionStage>(),
            [ProductionStage.FailedNeedsUser] = new HashSet<ProductionStage>(),
            [ProductionStage.FailedFatal] = new HashSet<ProductionStage>(),
            [ProductionStage.Completed] = new HashSet<ProductionStage>()
        };

    internal ProductionStateTransition Advance(
        ProductionJob job,
        IReadOnlyList<GenerationTask> tasks,
        DateTimeOffset now)
    {
        if (job.Status == ProductionJobStatus.Paused)
        {
            return PausedTransition(job, tasks);
        }

        if (job.Status == ProductionJobStatus.Completed)
        {
            return ArtifactReady(job);
        }

        if (job.Status == ProductionJobStatus.Failed)
        {
            return FailedTransition(job, tasks);
        }

        if (job.CurrentStage == ProductionStage.Created)
        {
            return EnterStage(job, tasks, ProductionStageCatalog.First, now, "task_started");
        }

        var currentStage = ProductionStageCatalog.Find(job.CurrentStage);

        if (currentStage is null)
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = "当前 production stage 无法识别。";

            return FailedTransition(job, tasks);
        }

        var currentTask = _taskQueue.FindTask(tasks, currentStage);

        if (currentTask is null)
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = $"stage {currentStage.Id} 缺少 generation task。";

            return FailedTransition(job, tasks);
        }

        return currentTask.Status switch
        {
            GenerationTaskStatus.Waiting => EnterStage(job, tasks, currentStage, now, "stage_changed"),
            GenerationTaskStatus.Running => CompleteOrCheckpoint(job, tasks, currentStage, currentTask, now),
            GenerationTaskStatus.Review => PauseForCheckpoint(job, currentStage, currentTask),
            GenerationTaskStatus.Completed => AdvancePastCompletedStage(job, tasks, currentStage, now),
            GenerationTaskStatus.Failed => MarkFailed(job, currentStage, currentTask),
            _ => EnterStage(job, tasks, currentStage, now, "stage_changed")
        };
    }

    internal void Pause(ProductionJob job)
    {
        if (job.Status is ProductionJobStatus.Running or ProductionJobStatus.Queued)
        {
            job.Status = ProductionJobStatus.Paused;
        }
    }

    internal bool Resume(ProductionJob job, IReadOnlyList<GenerationTask> tasks)
    {
        if (job.Status != ProductionJobStatus.Paused)
        {
            return false;
        }

        var stage = ProductionStageCatalog.Find(job.CurrentStage);
        var task = stage is null ? null : _taskQueue.FindTask(tasks, stage);

        if (stage?.NeedsReview == true && task?.Status == GenerationTaskStatus.Review)
        {
            job.ErrorMessage = "当前节点等待 checkpoint 确认，请先提交 checkpoint。";
            return false;
        }

        job.Status = ProductionJobStatus.Running;
        job.ErrorMessage = null;
        return true;
    }

    internal bool ApplyCheckpoint(
        ProductionJob job,
        IReadOnlyList<GenerationTask> tasks,
        bool approved,
        DateTimeOffset now,
        string? notes)
    {
        var stage = ProductionStageCatalog.Find(job.CurrentStage);

        if (stage is null || !stage.NeedsReview)
        {
            job.ErrorMessage = "当前 stage 没有可确认的 checkpoint。";
            return false;
        }

        var task = _taskQueue.FindTask(tasks, stage);

        if (task is null)
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = "checkpoint 对应 task 不存在。";
            return false;
        }

        if (!approved)
        {
            task.Status = GenerationTaskStatus.Failed;
            task.ErrorMessage = string.IsNullOrWhiteSpace(notes) ? "用户拒绝 checkpoint。" : notes;
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedRetryable;
            job.ErrorMessage = task.ErrorMessage;
            return false;
        }

        _taskQueue.MarkCompleted(task, now);
        job.Status = ProductionJobStatus.Running;
        job.ErrorMessage = null;
        return true;
    }

    internal void PrepareRetry(ProductionJob job, IReadOnlyList<GenerationTask> tasks)
    {
        var failedTasks = tasks.Where(task => task.Status == GenerationTaskStatus.Failed).ToList();

        if (failedTasks.Count == 0)
        {
            return;
        }

        _taskQueue.ResetFailedTasks(tasks);

        var retryStage = failedTasks
            .Select(task => ProductionStageCatalog.FindBySkill(task.SkillName))
            .Where(stage => stage is not null)
            .OrderBy(stage => ProductionStageCatalog.IndexOf(stage!.Stage))
            .FirstOrDefault();

        if (retryStage is null)
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = "无法定位可重试 task 对应的 stage。";
            return;
        }

        job.CurrentStage = retryStage.Stage;
        job.ProgressPercent = Math.Max(0, ProductionStageCatalog.ProgressFor(retryStage.Stage) - 1);
        job.Status = ProductionJobStatus.Running;
        job.FinishedAt = null;
        job.ErrorMessage = null;
    }

    private ProductionStateTransition CompleteOrCheckpoint(
        ProductionJob job,
        IReadOnlyList<GenerationTask> tasks,
        ProductionStageDefinition stage,
        GenerationTask task,
        DateTimeOffset now)
    {
        if (stage.NeedsReview)
        {
            return PauseForCheckpoint(job, stage, task);
        }

        _taskQueue.MarkCompleted(task, now);
        return AdvancePastCompletedStage(job, tasks, stage, now);
    }

    private ProductionStateTransition PauseForCheckpoint(
        ProductionJob job,
        ProductionStageDefinition stage,
        GenerationTask task)
    {
        _taskQueue.MarkReadyForReview(task);
        job.Status = ProductionJobStatus.Paused;
        job.ProgressPercent = ProductionStageCatalog.ProgressFor(stage.Stage);

        return new ProductionStateTransition(
            "checkpoint_required",
            stage.Id,
            stage.Label,
            stage.Skill,
            GenerationTaskStatus.Review,
            job.ProgressPercent,
            $"{stage.Label} 已生成，等待用户确认。",
            IsTerminal: false);
    }

    private ProductionStateTransition AdvancePastCompletedStage(
        ProductionJob job,
        IReadOnlyList<GenerationTask> tasks,
        ProductionStageDefinition stage,
        DateTimeOffset now)
    {
        var nextStage = NextStage(stage.Stage);

        if (nextStage == ProductionStage.Completed)
        {
            return CompleteJob(job, now);
        }

        if (!CanTransition(job.CurrentStage, nextStage))
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = $"非法状态迁移：{job.CurrentStage} -> {nextStage}";

            return FailedTransition(job, tasks);
        }

        var nextDefinition = ProductionStageCatalog.Find(nextStage);

        if (nextDefinition is null)
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = $"无法定位下一 stage：{nextStage}";

            return FailedTransition(job, tasks);
        }

        return EnterStage(job, tasks, nextDefinition, now, "stage_changed");
    }

    private ProductionStateTransition EnterStage(
        ProductionJob job,
        IReadOnlyList<GenerationTask> tasks,
        ProductionStageDefinition stage,
        DateTimeOffset now,
        string eventType)
    {
        if (!CanTransition(job.CurrentStage, stage.Stage) && job.CurrentStage != stage.Stage)
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = $"非法状态迁移：{job.CurrentStage} -> {stage.Stage}";

            return FailedTransition(job, tasks);
        }

        var task = _taskQueue.FindTask(tasks, stage);

        if (task is null)
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = $"stage {stage.Id} 缺少 generation task。";

            return FailedTransition(job, tasks);
        }

        _taskQueue.MarkStarted(task, now);
        job.CurrentStage = stage.Stage;
        job.Status = ProductionJobStatus.Running;
        job.ProgressPercent = ProductionStageCatalog.ProgressFor(stage.Stage);
        job.ErrorMessage = null;

        return new ProductionStateTransition(
            eventType,
            stage.Id,
            stage.Label,
            stage.Skill,
            GenerationTaskStatus.Running,
            job.ProgressPercent,
            $"{stage.Label} 正在执行。",
            IsTerminal: false);
    }

    private ProductionStateTransition PausedTransition(ProductionJob job, IReadOnlyList<GenerationTask> tasks)
    {
        var stage = ProductionStageCatalog.Find(job.CurrentStage);
        var task = stage is null ? null : _taskQueue.FindTask(tasks, stage);

        if (stage is not null && task?.Status == GenerationTaskStatus.Review)
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

        return new ProductionStateTransition(
            "stage_paused",
            ProductionStageCatalog.ExternalIdFor(job.CurrentStage),
            "任务已暂停",
            "control_plane",
            task?.Status ?? GenerationTaskStatus.Waiting,
            job.ProgressPercent,
            "生产任务已暂停。",
            IsTerminal: false);
    }

    private ProductionStateTransition MarkFailed(
        ProductionJob job,
        ProductionStageDefinition stage,
        GenerationTask task)
    {
        job.Status = ProductionJobStatus.Failed;
        job.CurrentStage = ProductionStage.FailedRetryable;
        job.ErrorMessage = task.ErrorMessage ?? $"{stage.Label} 执行失败，可重试。";

        return new ProductionStateTransition(
            "task_failed",
            stage.Id,
            stage.Label,
            stage.Skill,
            GenerationTaskStatus.Failed,
            job.ProgressPercent,
            job.ErrorMessage,
            IsTerminal: true);
    }

    private ProductionStateTransition FailedTransition(ProductionJob job, IReadOnlyList<GenerationTask> tasks)
    {
        var stage = tasks
            .Select(task => ProductionStageCatalog.FindBySkill(task.SkillName))
            .Where(definition => definition is not null)
            .OrderBy(definition => ProductionStageCatalog.IndexOf(definition!.Stage))
            .FirstOrDefault();

        return new ProductionStateTransition(
            "task_failed",
            stage?.Id ?? ProductionStageCatalog.ExternalIdFor(job.CurrentStage),
            stage?.Label ?? "生产失败",
            stage?.Skill ?? "control_plane",
            GenerationTaskStatus.Failed,
            job.ProgressPercent,
            job.ErrorMessage ?? "生产任务失败。",
            IsTerminal: true);
    }

    private ProductionStateTransition CompleteJob(ProductionJob job, DateTimeOffset now)
    {
        if (!CanTransition(job.CurrentStage, ProductionStage.Completed))
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedFatal;
            job.ErrorMessage = $"非法状态迁移：{job.CurrentStage} -> {ProductionStage.Completed}";

            return new ProductionStateTransition(
                "task_failed",
                "failed_fatal",
                "生产失败",
                "control_plane",
                GenerationTaskStatus.Failed,
                job.ProgressPercent,
                job.ErrorMessage,
                IsTerminal: true);
        }

        job.CurrentStage = ProductionStage.Completed;
        job.Status = ProductionJobStatus.Completed;
        job.ProgressPercent = 100;
        job.FinishedAt = now;
        job.ErrorMessage = null;

        return ArtifactReady(job);
    }

    private static ProductionStateTransition ArtifactReady(ProductionJob job)
    {
        return new ProductionStateTransition(
            "artifact_ready",
            "delivery",
            "导出包",
            "export_packager",
            GenerationTaskStatus.Completed,
            100,
            "mock 生产任务已完成，后续阶段会接入真实 Worker 和资产索引。",
            IsTerminal: true);
    }

    private static bool CanTransition(ProductionStage from, ProductionStage to)
    {
        return from == to || (LegalTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to));
    }

    private static ProductionStage NextStage(ProductionStage stage)
    {
        var index = ProductionStageCatalog.IndexOf(stage);

        if (index < 0 || index >= ProductionStageCatalog.All.Count - 1)
        {
            return ProductionStage.Completed;
        }

        return ProductionStageCatalog.All[index + 1].Stage;
    }
}

internal sealed record ProductionStateTransition(
    string EventType,
    string StageId,
    string StageLabel,
    string Skill,
    GenerationTaskStatus TaskStatus,
    int Progress,
    string Message,
    bool IsTerminal);
