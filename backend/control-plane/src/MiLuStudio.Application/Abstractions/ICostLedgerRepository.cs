namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Domain.Entities;

public interface ICostLedgerRepository
{
    Task<IReadOnlyList<CostLedgerEntry>> ListCostByProjectAsync(string projectId, CancellationToken cancellationToken);

    Task AddAsync(CostLedgerEntry entry, CancellationToken cancellationToken);
}
