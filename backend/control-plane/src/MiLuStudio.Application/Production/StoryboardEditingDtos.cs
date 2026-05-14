namespace MiLuStudio.Application.Production;

public sealed record StoryboardEditRequest(
    IReadOnlyList<StoryboardShotEdit> Shots,
    string? Notes);

public sealed record StoryboardShotRegenerateRequest(string? Notes);

public sealed record StoryboardShotEdit(
    string ShotId,
    int DurationSeconds,
    string Scene,
    string VisualAction,
    string ShotSize,
    string CameraMovement,
    string SoundNote,
    string Dialogue,
    string Narration);

public sealed record StoryboardEditResponse(
    string TaskId,
    string JobId,
    string ProjectId,
    string Status,
    int ResetDownstreamTaskCount,
    string Message);

public sealed class StoryboardEditValidationException : Exception
{
    public StoryboardEditValidationException(string message)
        : base(message)
    {
    }
}
