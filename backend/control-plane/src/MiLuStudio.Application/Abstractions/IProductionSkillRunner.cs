namespace MiLuStudio.Application.Abstractions;

public interface IProductionSkillRunner
{
    Task<ProductionSkillRunResult> RunAsync(
        string skillName,
        string inputJson,
        CancellationToken cancellationToken);
}

public sealed record ProductionSkillRunResult(
    string OutputJson,
    int ExitCode,
    string StandardOutput,
    string StandardError);
