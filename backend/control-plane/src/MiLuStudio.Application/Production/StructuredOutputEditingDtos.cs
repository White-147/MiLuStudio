namespace MiLuStudio.Application.Production;

using JsonElement = global::System.Text.Json.JsonElement;

public sealed record StructuredOutputEditRequest(
    IReadOnlyList<StructuredOutputFieldEdit> Edits,
    string? Notes);

public sealed record StructuredOutputFieldEdit(
    string Path,
    JsonElement Value);

public sealed record StructuredOutputEditResponse(
    string TaskId,
    string JobId,
    string ProjectId,
    string SkillName,
    string Status,
    int ResetDownstreamTaskCount,
    string Message);

public sealed class StructuredOutputEditValidationException : Exception
{
    public StructuredOutputEditValidationException(string message)
        : base(message)
    {
    }
}
