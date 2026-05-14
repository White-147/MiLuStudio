namespace MiLuStudio.Application.Projects;

using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class ProjectService
{
    public const int MinimumStoryLength = 500;
    public const int MaximumStoryLength = 2000;

    private static readonly HashSet<string> AllowedAspectRatios = new(StringComparer.OrdinalIgnoreCase)
    {
        "9:16",
        "16:9",
        "1:1"
    };

    private readonly IClock _clock;
    private readonly IProjectRepository _projects;
    private readonly IProductionJobRepository _jobs;

    public ProjectService(IClock clock, IProjectRepository projects, IProductionJobRepository jobs)
    {
        _clock = clock;
        _projects = projects;
        _jobs = jobs;
    }

    public async Task<IReadOnlyList<ProjectSummaryDto>> ListAsync(CancellationToken cancellationToken)
    {
        var projects = await _projects.ListAsync(cancellationToken);
        var summaries = new List<ProjectSummaryDto>(projects.Count);

        foreach (var project in projects.OrderByDescending(project => project.UpdatedAt))
        {
            var jobs = await _jobs.ListByProjectAsync(project.Id, cancellationToken);
            var latestJob = jobs.OrderByDescending(job => job.StartedAt).FirstOrDefault();
            summaries.Add(ToSummaryDto(project, latestJob));
        }

        return summaries;
    }

    public async Task<ProjectDetailDto?> GetAsync(string projectId, CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(projectId, cancellationToken);

        if (project is null)
        {
            return null;
        }

        var storyInput = await _projects.GetStoryInputAsync(project.Id, cancellationToken);
        var jobs = await _jobs.ListByProjectAsync(project.Id, cancellationToken);
        var latestJob = jobs.OrderByDescending(job => job.StartedAt).FirstOrDefault();

        return ToDetailDto(project, storyInput?.OriginalText ?? string.Empty, latestJob);
    }

    public async Task<ProjectDetailDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var now = _clock.Now;
        var title = NormalizeText(request.Title, "未命名漫剧");
        var storyText = NormalizeStoryText(request.StoryText);
        var mode = ParseMode(request.Mode);
        var targetDuration = NormalizeDuration(request.TargetDuration);
        var aspectRatio = NormalizeAspectRatio(request.AspectRatio);
        var stylePreset = NormalizeText(request.StylePreset, "轻写实国漫");

        var project = new Project
        {
            Id = CreateId("proj"),
            Name = title,
            Description = BuildDescription(storyText),
            Mode = mode,
            Status = ProjectStatus.Draft,
            TargetDurationSeconds = targetDuration,
            AspectRatio = aspectRatio,
            StylePreset = stylePreset,
            CreatedAt = now,
            UpdatedAt = now
        };

        var storyInput = new StoryInput
        {
            Id = CreateId("story"),
            ProjectId = project.Id,
            SourceType = "text",
            OriginalText = storyText,
            Language = "zh-CN",
            WordCount = CountWords(storyText)
        };

        await _projects.AddAsync(project, storyInput, cancellationToken);

        return ToDetailDto(project, storyText, latestJob: null);
    }

    public async Task<ProjectDetailDto?> UpdateAsync(string projectId, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(projectId, cancellationToken);

        if (project is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            project.Name = request.Title.Trim();
        }

        var storyInput = await _projects.GetStoryInputAsync(project.Id, cancellationToken);
        var storyTextChanged = request.StoryText is not null;

        if (storyTextChanged)
        {
            var storyText = NormalizeStoryText(request.StoryText);
            storyInput ??= new StoryInput
            {
                Id = CreateId("story"),
                ProjectId = project.Id,
                SourceType = "text",
                OriginalText = storyText,
                Language = "zh-CN",
                WordCount = CountWords(storyText)
            };

            storyInput.OriginalText = storyText;
            storyInput.WordCount = CountWords(storyText);
            storyInput.ParsedAt = null;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            project.Description = request.Description.Trim();
        }
        else if (storyTextChanged && storyInput is not null)
        {
            project.Description = BuildDescription(storyInput.OriginalText);
        }

        if (!string.IsNullOrWhiteSpace(request.Mode))
        {
            project.Mode = ParseMode(request.Mode);
        }

        if (request.TargetDuration.HasValue)
        {
            project.TargetDurationSeconds = NormalizeDuration(request.TargetDuration);
        }

        if (!string.IsNullOrWhiteSpace(request.AspectRatio))
        {
            project.AspectRatio = NormalizeAspectRatio(request.AspectRatio);
        }

        if (!string.IsNullOrWhiteSpace(request.StylePreset))
        {
            project.StylePreset = request.StylePreset.Trim();
        }

        project.UpdatedAt = _clock.Now;

        if (storyTextChanged && storyInput is not null)
        {
            await _projects.UpdateAsync(project, storyInput, cancellationToken);
        }
        else
        {
            await _projects.UpdateAsync(project, cancellationToken);
        }

        var jobs = await _jobs.ListByProjectAsync(project.Id, cancellationToken);
        var latestJob = jobs.OrderByDescending(job => job.StartedAt).FirstOrDefault();

        return ToDetailDto(project, storyInput?.OriginalText ?? string.Empty, latestJob);
    }

    public async Task<bool> DeleteAsync(string projectId, CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(projectId, cancellationToken);

        if (project is null)
        {
            return false;
        }

        var jobs = await _jobs.ListByProjectAsync(project.Id, cancellationToken);
        var hasActiveJob = jobs.Any(job => job.Status is ProductionJobStatus.Queued or ProductionJobStatus.Running or ProductionJobStatus.Paused);

        if (project.Status is ProjectStatus.Running or ProjectStatus.Paused || hasActiveJob)
        {
            throw new ProjectDeleteNotAllowedException("项目正在生成或暂停审核中，请等待完成、失败后再删除。");
        }

        await _projects.DeleteAsync(project.Id, cancellationToken);
        return true;
    }

    private static ProjectSummaryDto ToSummaryDto(Project project, ProductionJob? latestJob)
    {
        return new ProjectSummaryDto(
            project.Id,
            project.Name,
            project.Description,
            FormatMode(project.Mode),
            FormatProjectStatus(project.Status),
            project.TargetDurationSeconds,
            project.AspectRatio,
            FormatDate(project.UpdatedAt),
            latestJob?.ProgressPercent ?? ProjectProgress(project.Status));
    }

    private static ProjectDetailDto ToDetailDto(Project project, string storyText, ProductionJob? latestJob)
    {
        return new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            FormatMode(project.Mode),
            FormatProjectStatus(project.Status),
            project.TargetDurationSeconds,
            project.AspectRatio,
            project.StylePreset,
            FormatDate(project.UpdatedAt),
            storyText,
            latestJob?.ProgressPercent ?? ProjectProgress(project.Status));
    }

    private static ProjectMode ParseMode(string? mode)
    {
        return string.Equals(mode, "fast", StringComparison.OrdinalIgnoreCase)
            ? ProjectMode.Fast
            : ProjectMode.Director;
    }

    private static string FormatMode(ProjectMode mode)
    {
        return mode == ProjectMode.Fast ? "fast" : "director";
    }

    private static string FormatProjectStatus(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Draft => "draft",
            ProjectStatus.Running => "running",
            ProjectStatus.Paused => "paused",
            ProjectStatus.Completed => "completed",
            ProjectStatus.Failed => "failed",
            _ => "draft"
        };
    }

    private static int ProjectProgress(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Completed => 100,
            ProjectStatus.Running => 18,
            ProjectStatus.Paused => 18,
            ProjectStatus.Failed => 0,
            _ => 0
        };
    }

    private static string NormalizeAspectRatio(string? aspectRatio)
    {
        var value = string.IsNullOrWhiteSpace(aspectRatio) ? "9:16" : aspectRatio.Trim();

        return AllowedAspectRatios.Contains(value) ? value : "9:16";
    }

    private static int NormalizeDuration(int? targetDuration)
    {
        return Math.Clamp(targetDuration ?? 45, 30, 60);
    }

    private static string NormalizeStoryText(string? value)
    {
        var storyText = NormalizeText(value, string.Empty);
        var length = CountWords(storyText);

        if (length < MinimumStoryLength || length > MaximumStoryLength)
        {
            throw new ProjectValidationException(
                $"故事正文建议保持在 {MinimumStoryLength} 到 {MaximumStoryLength} 个非空白字符之间。",
                [
                    $"当前正文长度：{length}",
                    "请补足故事正文后再保存或启动生产任务。"
                ]);
        }

        return storyText;
    }

    private static string NormalizeText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string BuildDescription(string storyText)
    {
        var normalized = storyText.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= 54 ? normalized : $"{normalized[..54]}...";
    }

    private static int CountWords(string storyText)
    {
        return storyText.Count(character => !char.IsWhiteSpace(character));
    }

    private static string CreateId(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }
}

public sealed class ProjectValidationException : Exception
{
    public ProjectValidationException(string message, IReadOnlyList<string> details)
        : base(message)
    {
        Details = details;
    }

    public IReadOnlyList<string> Details { get; }
}

public sealed class ProjectDeleteNotAllowedException : Exception
{
    public ProjectDeleteNotAllowedException(string message)
        : base(message)
    {
    }
}
