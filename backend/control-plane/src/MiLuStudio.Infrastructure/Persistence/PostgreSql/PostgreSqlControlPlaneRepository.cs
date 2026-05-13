namespace MiLuStudio.Infrastructure.Persistence.PostgreSql;

using Microsoft.EntityFrameworkCore;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class PostgreSqlControlPlaneRepository :
    IProjectRepository,
    IProductionJobRepository,
    IAssetRepository,
    ICostLedgerRepository
{
    private readonly MiLuStudioDbContext _db;

    public PostgreSqlControlPlaneRepository(MiLuStudioDbContext db)
    {
        _db = db;
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
        var task = await _db.GenerationTasks
            .FromSqlInterpolated(
                $"""
                select gt.*
                from generation_tasks gt
                join production_jobs pj on pj.id = gt.job_id
                where (
                    gt.status = 'waiting'
                    or (gt.status = 'running' and (gt.locked_until is null or gt.locked_until <= {now}))
                  )
                  and pj.status in ('queued', 'running')
                  and not exists (
                    select 1
                    from generation_tasks previous
                    where previous.job_id = gt.job_id
                      and previous.queue_index < gt.queue_index
                      and previous.status <> 'completed'
                  )
                order by pj.started_at, gt.queue_index, gt.id
                for update skip locked
                limit 1
                """
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (task is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        task.Status = GenerationTaskStatus.Running;
        task.AttemptCount++;
        task.StartedAt ??= now;
        task.FinishedAt = null;
        task.LockedBy = workerId;
        task.LockedUntil = now.Add(leaseDuration);
        task.LastHeartbeatAt = now;
        task.ErrorMessage = null;

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _db.Entry(task).State = EntityState.Detached;

        return task;
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
