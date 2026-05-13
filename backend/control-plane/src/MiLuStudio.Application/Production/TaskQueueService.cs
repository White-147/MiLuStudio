namespace MiLuStudio.Application.Production;

using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class TaskQueueService
{
    internal IReadOnlyList<GenerationTask> CreateInitialTasks(
        string jobId,
        string projectId,
        string? requestedBy)
    {
        return ProductionStageCatalog.All
            .Select((stage, index) => new GenerationTask
            {
                Id = $"task_{Guid.NewGuid():N}",
                JobId = jobId,
                ProjectId = projectId,
                SkillName = stage.Skill,
                Provider = "mock-control-plane",
                InputJson = $$"""{"stage":"{{stage.Id}}","requestedBy":"{{requestedBy ?? "ui"}}"}""",
                Status = GenerationTaskStatus.Waiting,
                AttemptCount = 0,
                CostEstimate = index < 3 ? 0.01m * (index + 1) : 0
            })
            .ToList();
    }

    internal GenerationTask? FindTask(IReadOnlyList<GenerationTask> tasks, ProductionStageDefinition stage)
    {
        return tasks.FirstOrDefault(task => string.Equals(task.SkillName, stage.Skill, StringComparison.OrdinalIgnoreCase));
    }

    internal void MarkStarted(GenerationTask task, DateTimeOffset now)
    {
        if (task.Status != GenerationTaskStatus.Running)
        {
            task.AttemptCount++;
        }

        task.Status = GenerationTaskStatus.Running;
        task.StartedAt ??= now;
        task.FinishedAt = null;
        task.ErrorMessage = null;
    }

    internal void MarkReadyForReview(GenerationTask task)
    {
        task.Status = GenerationTaskStatus.Review;
    }

    internal void MarkCompleted(GenerationTask task, DateTimeOffset now)
    {
        task.Status = GenerationTaskStatus.Completed;
        task.FinishedAt = now;
        task.ErrorMessage = null;
    }

    internal bool ResetFailedTasks(IReadOnlyList<GenerationTask> tasks)
    {
        var changed = false;

        foreach (var task in tasks.Where(task => task.Status == GenerationTaskStatus.Failed))
        {
            task.Status = GenerationTaskStatus.Waiting;
            task.StartedAt = null;
            task.FinishedAt = null;
            task.ErrorMessage = null;
            changed = true;
        }

        return changed;
    }
}
