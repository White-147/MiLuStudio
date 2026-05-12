namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Domain.Entities;

public interface IProductionJobRepository
{
    Task<ProductionJob?> GetAsync(string jobId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionJob>> ListByProjectAsync(string projectId, CancellationToken cancellationToken);

    Task AddAsync(ProductionJob job, IReadOnlyList<GenerationTask> tasks, CancellationToken cancellationToken);

    Task UpdateAsync(ProductionJob job, CancellationToken cancellationToken);

    Task<IReadOnlyList<GenerationTask>> ListTasksAsync(string jobId, CancellationToken cancellationToken);

    Task ReplaceTasksAsync(string jobId, IReadOnlyList<GenerationTask> tasks, CancellationToken cancellationToken);
}
