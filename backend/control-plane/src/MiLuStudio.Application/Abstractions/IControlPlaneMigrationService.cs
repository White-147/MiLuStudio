namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Application.System;

public interface IControlPlaneMigrationService
{
    Task<MigrationStatusDto> GetStatusAsync(CancellationToken cancellationToken);

    Task<MigrationApplyResultDto> ApplyPendingAsync(CancellationToken cancellationToken);
}
