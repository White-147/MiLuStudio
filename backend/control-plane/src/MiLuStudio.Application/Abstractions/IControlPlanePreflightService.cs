namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Application.System;

public interface IControlPlanePreflightService
{
    Task<ControlPlanePreflightDto> CheckAsync(CancellationToken cancellationToken);
}
