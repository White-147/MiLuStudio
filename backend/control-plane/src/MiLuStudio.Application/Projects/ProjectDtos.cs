namespace MiLuStudio.Application.Projects;

public sealed record ProjectSummaryDto(
    string Id,
    string Title,
    string Description,
    string Mode,
    string Status,
    int TargetDuration,
    string AspectRatio,
    string UpdatedAt,
    int Progress);

public sealed record ProjectDetailDto(
    string Id,
    string Title,
    string Description,
    string Mode,
    string Status,
    int TargetDuration,
    string AspectRatio,
    string StylePreset,
    string UpdatedAt,
    string StoryText,
    int Progress);

public sealed record CreateProjectRequest(
    string? Title,
    string? StoryText,
    string? Mode,
    int? TargetDuration,
    string? AspectRatio,
    string? StylePreset);

public sealed record UpdateProjectRequest(
    string? Title,
    string? Description,
    string? Mode,
    int? TargetDuration,
    string? AspectRatio,
    string? StylePreset);
