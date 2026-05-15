namespace MiLuStudio.Infrastructure.Persistence.Sqlite;

using Microsoft.EntityFrameworkCore;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class SqliteControlPlaneRepository :
    IProjectRepository,
    IProductionJobRepository,
    IAssetRepository,
    ICostLedgerRepository
{
    private readonly MiLuStudioDbContext _db;

    public SqliteControlPlaneRepository(MiLuStudioDbContext db)
    {
        _db = db;
        _db.Database.EnsureCreated();
    }

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken)
    {
        return await _db.Projects
            .AsNoTracking()
            .OrderByDescending(project => project.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    async Task<Project?> IProjectRepository.GetAsync(string projectId, CancellationToken cancellationToken)
    {
        return await _db.Projects.AsNoTracking().FirstOrDefaultAsync(project => project.Id == projectId, cancellationToken);
    }

    public async Task<StoryInput?> GetStoryInputAsync(string projectId, CancellationToken cancellationToken)
    {
        return await _db.StoryInputs.AsNoTracking().FirstOrDefaultAsync(story => story.ProjectId == projectId, cancellationToken);
    }

    public async Task AddAsync(Project project, StoryInput storyInput, CancellationToken cancellationToken)
    {
        _db.Projects.Add(project);
        _db.StoryInputs.Add(storyInput);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken)
    {
        _db.Projects.Update(project);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task UpdateAsync(Project project, StoryInput storyInput, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        _db.Projects.Update(project);

        var storyExists = await _db.StoryInputs
            .AsNoTracking()
            .AnyAsync(story => story.Id == storyInput.Id || story.ProjectId == project.Id, cancellationToken);

        if (storyExists)
        {
            _db.StoryInputs.Update(storyInput);
        }
        else
        {
            _db.StoryInputs.Add(storyInput);
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task<bool> DeleteAsync(string projectId, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(project => project.Id == projectId, cancellationToken);

        if (project is null)
        {
            return false;
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return true;
    }

    async Task<ProductionJob?> IProductionJobRepository.GetAsync(string jobId, CancellationToken cancellationToken)
    {
        return await _db.ProductionJobs.AsNoTracking().FirstOrDefaultAsync(job => job.Id == jobId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionJob>> ListByProjectAsync(string projectId, CancellationToken cancellationToken)
    {
        return await _db.ProductionJobs
            .AsNoTracking()
            .Where(job => job.ProjectId == projectId)
            .OrderByDescending(job => job.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductionJob job, IReadOnlyList<GenerationTask> tasks, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        _db.ProductionJobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);

        _db.GenerationTasks.AddRange(tasks);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task UpdateAsync(ProductionJob job, CancellationToken cancellationToken)
    {
        _db.ProductionJobs.Update(job);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task<IReadOnlyList<GenerationTask>> ListTasksAsync(string jobId, CancellationToken cancellationToken)
    {
        return await _db.GenerationTasks
            .AsNoTracking()
            .Where(task => task.JobId == jobId)
            .OrderBy(task => task.QueueIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceTasksAsync(string jobId, IReadOnlyList<GenerationTask> tasks, CancellationToken cancellationToken)
    {
        var existingIds = await _db.GenerationTasks
            .AsNoTracking()
            .Where(task => task.JobId == jobId)
            .Select(task => task.Id)
            .ToListAsync(cancellationToken);
        var existingIdSet = existingIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var task in tasks)
        {
            if (existingIdSet.Contains(task.Id))
            {
                _db.GenerationTasks.Update(task);
            }
            else
            {
                _db.GenerationTasks.Add(task);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task<GenerationTask?> GetTaskAsync(string taskId, CancellationToken cancellationToken)
    {
        return await _db.GenerationTasks.AsNoTracking().FirstOrDefaultAsync(task => task.Id == taskId, cancellationToken);
    }

    public async Task UpdateTaskAsync(GenerationTask task, CancellationToken cancellationToken)
    {
        _db.GenerationTasks.Update(task);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task<GenerationTask?> ClaimNextTaskAsync(
        string workerId,
        DateTimeOffset now,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var task = await (
                from candidate in _db.GenerationTasks.AsNoTracking()
                join job in _db.ProductionJobs.AsNoTracking() on candidate.JobId equals job.Id
                where
                    (candidate.Status == GenerationTaskStatus.Waiting ||
                        (candidate.Status == GenerationTaskStatus.Running &&
                            (candidate.LockedUntil == null || candidate.LockedUntil <= now))) &&
                    (job.Status == ProductionJobStatus.Queued || job.Status == ProductionJobStatus.Running) &&
                    !_db.GenerationTasks.Any(previous =>
                        previous.JobId == candidate.JobId &&
                        previous.QueueIndex < candidate.QueueIndex &&
                        previous.Status != GenerationTaskStatus.Completed)
                orderby job.StartedAt, candidate.QueueIndex, candidate.Id
                select candidate)
            .FirstOrDefaultAsync(cancellationToken);

        if (task is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        var claimedUntil = now.Add(leaseDuration);
        var startedAt = task.StartedAt ?? now;
        var updated = await _db.GenerationTasks
            .Where(candidate =>
                candidate.Id == task.Id &&
                (candidate.Status == GenerationTaskStatus.Waiting ||
                    (candidate.Status == GenerationTaskStatus.Running &&
                        (candidate.LockedUntil == null || candidate.LockedUntil <= now))))
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(candidate => candidate.Status, GenerationTaskStatus.Running)
                .SetProperty(candidate => candidate.AttemptCount, candidate => candidate.AttemptCount + 1)
                .SetProperty(candidate => candidate.StartedAt, startedAt)
                .SetProperty(candidate => candidate.FinishedAt, (DateTimeOffset?)null)
                .SetProperty(candidate => candidate.LockedBy, workerId)
                .SetProperty(candidate => candidate.LockedUntil, claimedUntil)
                .SetProperty(candidate => candidate.LastHeartbeatAt, now)
                .SetProperty(candidate => candidate.ErrorMessage, (string?)null),
                cancellationToken);

        if (updated == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        var claimed = await _db.GenerationTasks
            .AsNoTracking()
            .FirstAsync(candidate => candidate.Id == task.Id, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return claimed;
    }

    public async Task<IReadOnlyList<Asset>> ListAssetsByProjectAsync(string projectId, CancellationToken cancellationToken)
    {
        return await _db.Assets
            .AsNoTracking()
            .Where(asset => asset.ProjectId == projectId)
            .OrderByDescending(asset => asset.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken)
    {
        _db.Assets.Add(asset);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }

    public async Task<IReadOnlyList<CostLedgerEntry>> ListCostByProjectAsync(string projectId, CancellationToken cancellationToken)
    {
        return await _db.CostLedger
            .AsNoTracking()
            .Where(entry => entry.ProjectId == projectId)
            .OrderByDescending(entry => entry.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CostLedgerEntry entry, CancellationToken cancellationToken)
    {
        _db.CostLedger.Add(entry);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }
}
