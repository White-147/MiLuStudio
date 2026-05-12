namespace MiLuStudio.Infrastructure.Persistence.InMemory;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class InMemoryControlPlaneStore : IProjectRepository, IProductionJobRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<string, Project> _projects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, StoryInput> _storyInputs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ProductionJob> _jobs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<GenerationTask>> _tasksByJob = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryControlPlaneStore()
    {
        Seed();
    }

    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<Project>>(_projects.Values.Select(Clone).ToList());
        }
    }

    Task<Project?> IProjectRepository.GetAsync(string projectId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_projects.TryGetValue(projectId, out var project) ? Clone(project) : null);
        }
    }

    public Task<StoryInput?> GetStoryInputAsync(string projectId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_storyInputs.TryGetValue(projectId, out var storyInput) ? Clone(storyInput) : null);
        }
    }

    public Task AddAsync(Project project, StoryInput storyInput, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _projects[project.Id] = Clone(project);
            _storyInputs[project.Id] = Clone(storyInput);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _projects[project.Id] = Clone(project);
        }

        return Task.CompletedTask;
    }

    Task<ProductionJob?> IProductionJobRepository.GetAsync(string jobId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_jobs.TryGetValue(jobId, out var job) ? Clone(job) : null);
        }
    }

    public Task<IReadOnlyList<ProductionJob>> ListByProjectAsync(string projectId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<ProductionJob>>(
                _jobs.Values
                    .Where(job => string.Equals(job.ProjectId, projectId, StringComparison.OrdinalIgnoreCase))
                    .Select(Clone)
                    .ToList());
        }
    }

    public Task AddAsync(ProductionJob job, IReadOnlyList<GenerationTask> tasks, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _jobs[job.Id] = Clone(job);
            _tasksByJob[job.Id] = tasks.Select(Clone).ToList();
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProductionJob job, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _jobs[job.Id] = Clone(job);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GenerationTask>> ListTasksAsync(string jobId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<GenerationTask>>(
                _tasksByJob.TryGetValue(jobId, out var tasks)
                    ? tasks.Select(Clone).ToList()
                    : []);
        }
    }

    public Task ReplaceTasksAsync(string jobId, IReadOnlyList<GenerationTask> tasks, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _tasksByJob[jobId] = tasks.Select(Clone).ToList();
        }

        return Task.CompletedTask;
    }

    private void Seed()
    {
        var now = DateTimeOffset.Now;
        AddSeedProject(
            new Project
            {
                Id = "demo-episode-01",
                Name = "雨巷里的纸鹤",
                Description = "悬疑都市短篇，女孩追踪会发光的纸鹤，发现失踪哥哥留下的线索。",
                Mode = ProjectMode.Director,
                Status = ProjectStatus.Running,
                TargetDurationSeconds = 45,
                AspectRatio = "9:16",
                StylePreset = "雨夜悬疑国漫",
                CreatedAt = now.AddHours(-6),
                UpdatedAt = now.AddMinutes(-24)
            },
            "雨夜里，林溪在旧巷口捡到一只会发光的纸鹤。纸鹤不断飞向废弃照相馆，那里藏着哥哥失踪前留下的最后一卷胶片。");

        AddSeedProject(
            new Project
            {
                Id = "demo-episode-02",
                Name = "星港便利店",
                Description = "轻喜剧科幻，一家夜班便利店每天凌晨都会接待不同星球的客人。",
                Mode = ProjectMode.Fast,
                Status = ProjectStatus.Draft,
                TargetDurationSeconds = 35,
                AspectRatio = "9:16",
                StylePreset = "明亮科幻漫画",
                CreatedAt = now.AddHours(-8),
                UpdatedAt = now.AddHours(-2)
            },
            "一家夜班便利店开在星港边缘，每天凌晨都会接待不同星球的客人。");

        AddSeedProject(
            new Project
            {
                Id = "demo-episode-03",
                Name = "长安旧梦",
                Description = "古风奇幻，画师进入一幅未完成的壁画，替画中少年改写结局。",
                Mode = ProjectMode.Director,
                Status = ProjectStatus.Completed,
                TargetDurationSeconds = 58,
                AspectRatio = "9:16",
                StylePreset = "古风壁画质感",
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddHours(-18)
            },
            "画师进入一幅未完成的壁画，替画中少年改写早已注定的结局。");
    }

    private void AddSeedProject(Project project, string storyText)
    {
        _projects[project.Id] = Clone(project);
        _storyInputs[project.Id] = new StoryInput
        {
            Id = $"story_{project.Id}",
            ProjectId = project.Id,
            SourceType = "text",
            OriginalText = storyText,
            Language = "zh-CN",
            WordCount = storyText.Count(character => !char.IsWhiteSpace(character))
        };
    }

    private static Project Clone(Project project)
    {
        return new Project
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Mode = project.Mode,
            Status = project.Status,
            TargetDurationSeconds = project.TargetDurationSeconds,
            AspectRatio = project.AspectRatio,
            StylePreset = project.StylePreset,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }

    private static StoryInput Clone(StoryInput storyInput)
    {
        return new StoryInput
        {
            Id = storyInput.Id,
            ProjectId = storyInput.ProjectId,
            SourceType = storyInput.SourceType,
            OriginalText = storyInput.OriginalText,
            FileAssetId = storyInput.FileAssetId,
            Language = storyInput.Language,
            WordCount = storyInput.WordCount,
            ParsedAt = storyInput.ParsedAt
        };
    }

    private static ProductionJob Clone(ProductionJob job)
    {
        return new ProductionJob
        {
            Id = job.Id,
            ProjectId = job.ProjectId,
            CurrentStage = job.CurrentStage,
            Status = job.Status,
            ProgressPercent = job.ProgressPercent,
            StartedAt = job.StartedAt,
            FinishedAt = job.FinishedAt,
            ErrorMessage = job.ErrorMessage
        };
    }

    private static GenerationTask Clone(GenerationTask task)
    {
        return new GenerationTask
        {
            Id = task.Id,
            JobId = task.JobId,
            ProjectId = task.ProjectId,
            ShotId = task.ShotId,
            SkillName = task.SkillName,
            Provider = task.Provider,
            InputJson = task.InputJson,
            OutputJson = task.OutputJson,
            Status = task.Status,
            AttemptCount = task.AttemptCount,
            CostEstimate = task.CostEstimate,
            CostActual = task.CostActual,
            StartedAt = task.StartedAt,
            FinishedAt = task.FinishedAt,
            ErrorMessage = task.ErrorMessage
        };
    }
}
