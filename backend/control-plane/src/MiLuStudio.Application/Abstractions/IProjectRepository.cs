namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Domain.Entities;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken);

    Task<Project?> GetAsync(string projectId, CancellationToken cancellationToken);

    Task<StoryInput?> GetStoryInputAsync(string projectId, CancellationToken cancellationToken);

    Task AddAsync(Project project, StoryInput storyInput, CancellationToken cancellationToken);

    Task UpdateAsync(Project project, CancellationToken cancellationToken);

    Task UpdateAsync(Project project, StoryInput storyInput, CancellationToken cancellationToken);
}
