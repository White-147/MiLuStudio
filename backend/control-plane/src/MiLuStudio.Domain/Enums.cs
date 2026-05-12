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

public enum GenerationTaskStatus
{
    Waiting,
    Running,
    Review,
    Completed,
    Failed
}
