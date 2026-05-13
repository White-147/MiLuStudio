namespace MiLuStudio.Application.System;

public sealed record ControlPlanePreflightDto(
    string RepositoryProvider,
    bool Healthy,
    IReadOnlyList<PreflightCheckDto> Checks,
    IReadOnlyList<string> Recommendations);

public sealed record PreflightCheckDto(
    string Name,
    string Status,
    string Message,
    IReadOnlyDictionary<string, string> Details);

public sealed record MigrationStatusDto(
    string RepositoryProvider,
    string Status,
    IReadOnlyList<MigrationFileDto> Migrations);

public sealed record MigrationFileDto(
    string Id,
    string FileName,
    string Status,
    DateTimeOffset? AppliedAt);

public sealed record MigrationApplyResultDto(
    string RepositoryProvider,
    string Status,
    IReadOnlyList<string> AppliedMigrationIds,
    IReadOnlyList<string> SkippedMigrationIds);
