namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Domain.Entities;

public interface IAssetRepository
{
    Task<IReadOnlyList<Asset>> ListAssetsByProjectAsync(string projectId, CancellationToken cancellationToken);

    Task AddAsync(Asset asset, CancellationToken cancellationToken);
}
