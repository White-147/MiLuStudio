namespace MiLuStudio.Domain;

public enum ProjectMode
{
    Fast,
    Director
}

public enum ProjectStatus
{
    Draft,
    Running,
    Paused,
    Completed,
    Failed
}

public enum ProductionJobStatus
{
    Queued,
    Running,
    Paused,
    Completed,
    Failed
}

public enum ProductionStage
{
    Created,
    StoryIngesting,
    PlotAdapted,
    ScriptReadyForReview,
    CharacterReadyForReview,
    StyleReadyForReview,
    StoryboardReadyForReview,
    ImagePromptsReady,
    ImagesReadyForReview,
    VideoPromptsReady,
    VideosReadyForReview,
    AudioReadyForReview,
    SubtitlesReady,
    EditReadyForQualityCheck,
    QualityReadyForReview,
    Exporting,
    Completed,
    FailedRetryable,
    FailedNeedsUser,
    FailedFatal
}

public enum GenerationTaskStatus
{
    Waiting,
    Running,
    Review,
    Completed,
    Failed
}
