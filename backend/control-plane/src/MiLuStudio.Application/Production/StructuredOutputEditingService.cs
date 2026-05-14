namespace MiLuStudio.Application.Production;

using CultureInfo = global::System.Globalization.CultureInfo;
using DateTimeOffset = global::System.DateTimeOffset;
using JsonArray = global::System.Text.Json.Nodes.JsonArray;
using JsonNode = global::System.Text.Json.Nodes.JsonNode;
using JsonObject = global::System.Text.Json.Nodes.JsonObject;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class StructuredOutputEditingService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> EditableFields =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["character_bible"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "characters",
                "relationship_notes",
                "continuity_rules"
            },
            ["style_bible"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "style_name",
                "visual_style",
                "color_palette",
                "camera_language",
                "negative_prompt",
                "reusable_prompt_blocks",
                "image_prompt_guidelines",
                "video_prompt_guidelines",
                "continuity_notes"
            },
            ["image_prompt_builder"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image_requests",
                "negative_prompt",
                "reference_strategy"
            },
            ["video_prompt_builder"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "video_requests",
                "negative_prompt",
                "source_asset_manifest"
            }
        };

    private readonly IClock _clock;
    private readonly IProductionJobRepository _jobs;
    private readonly IProjectRepository _projects;

    public StructuredOutputEditingService(
        IClock clock,
        IProductionJobRepository jobs,
        IProjectRepository projects)
    {
        _clock = clock;
        _jobs = jobs;
        _projects = projects;
    }

    public async Task<StructuredOutputEditResponse?> SaveAsync(
        string taskId,
        StructuredOutputEditRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Edits.Count == 0)
        {
            throw new StructuredOutputEditValidationException("At least one editable field is required.");
        }

        var task = await _jobs.GetTaskAsync(taskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var context = await LoadContextAsync(task, cancellationToken);
        var allowedFields = EditableFields[task.SkillName];
        var editedFields = new List<string>();

        foreach (var edit in request.Edits)
        {
            var fieldName = NormalizeFieldName(edit.Path);
            if (!allowedFields.Contains(fieldName))
            {
                throw new StructuredOutputEditValidationException(
                    $"{task.SkillName} does not allow editing field '{edit.Path}'.");
            }

            var value = JsonNode.Parse(edit.Value.GetRawText())
                ?? throw new StructuredOutputEditValidationException($"Field '{edit.Path}' cannot be null.");
            context.Data[fieldName] = value;
            editedFields.Add(fieldName);
        }

        EnsureRequiredFields(context.Data, task.SkillName);
        UpdateReviewMetadata(context.Data, task.SkillName, editedFields, request.Notes, context.Stage.NeedsReview, _clock.Now);

        return await PersistAsync(context, editedFields, request.Notes, cancellationToken);
    }

    private async Task<StructuredOutputEditContext> LoadContextAsync(
        GenerationTask task,
        CancellationToken cancellationToken)
    {
        if (!EditableFields.ContainsKey(task.SkillName))
        {
            throw new StructuredOutputEditValidationException(
                "Only character, style, image prompt, and video prompt outputs can be edited in Stage 21.");
        }

        if (string.IsNullOrWhiteSpace(task.OutputJson))
        {
            throw new StructuredOutputEditValidationException("The selected output has not been generated yet.");
        }

        var stage = ProductionStageCatalog.FindBySkill(task.SkillName)
            ?? throw new StructuredOutputEditValidationException("The selected output is not attached to a production stage.");
        var job = await _jobs.GetAsync(task.JobId, cancellationToken)
            ?? throw new StructuredOutputEditValidationException("The production job no longer exists.");
        var project = await _projects.GetAsync(task.ProjectId, cancellationToken)
            ?? throw new StructuredOutputEditValidationException("The project no longer exists.");
        var tasks = (await _jobs.ListTasksAsync(task.JobId, cancellationToken)).ToList();
        var envelope = JsonNode.Parse(task.OutputJson)?.AsObject()
            ?? throw new StructuredOutputEditValidationException("The selected output is not an editable JSON envelope.");

        if (envelope["ok"]?.GetValue<bool>() != true)
        {
            throw new StructuredOutputEditValidationException("Failed outputs must be regenerated before editing.");
        }

        if (!string.Equals(ReadString(envelope, "skill_name"), task.SkillName, StringComparison.OrdinalIgnoreCase))
        {
            throw new StructuredOutputEditValidationException("The selected output skill does not match its task record.");
        }

        var data = envelope["data"]?.AsObject()
            ?? throw new StructuredOutputEditValidationException("The selected output is missing data.");

        return new StructuredOutputEditContext(job, project, task, tasks, envelope, data, stage);
    }

    private async Task<StructuredOutputEditResponse> PersistAsync(
        StructuredOutputEditContext context,
        IReadOnlyList<string> editedFields,
        string? notes,
        CancellationToken cancellationToken)
    {
        var now = _clock.Now;
        context.Task.OutputJson = context.Envelope.ToJsonString();
        context.Task.Status = context.Stage.NeedsReview
            ? GenerationTaskStatus.Review
            : GenerationTaskStatus.Completed;
        context.Task.StartedAt ??= now;
        context.Task.FinishedAt = context.Stage.NeedsReview ? null : now;
        context.Task.LockedBy = null;
        context.Task.LockedUntil = null;
        context.Task.LastHeartbeatAt = now;
        context.Task.CheckpointNotes = NormalizeNotes(notes);
        context.Task.ErrorMessage = null;

        var resetCount = 0;
        foreach (var downstream in context.Tasks.Where(task => task.QueueIndex > context.Task.QueueIndex))
        {
            if (downstream.Status != GenerationTaskStatus.Waiting ||
                downstream.OutputJson is not null ||
                downstream.StartedAt is not null ||
                downstream.FinishedAt is not null ||
                downstream.ErrorMessage is not null)
            {
                resetCount++;
            }

            downstream.Status = GenerationTaskStatus.Waiting;
            downstream.OutputJson = null;
            downstream.StartedAt = null;
            downstream.FinishedAt = null;
            downstream.LockedBy = null;
            downstream.LockedUntil = null;
            downstream.LastHeartbeatAt = null;
            downstream.CheckpointNotes = null;
            downstream.ErrorMessage = null;
        }

        var index = context.Tasks.FindIndex(task => string.Equals(task.Id, context.Task.Id, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            context.Tasks[index] = context.Task;
        }

        context.Job.CurrentStage = context.Stage.Stage;
        context.Job.ProgressPercent = ProductionStageCatalog.ProgressFor(context.Stage.Stage);
        context.Job.Status = context.Stage.NeedsReview ? ProductionJobStatus.Paused : ProductionJobStatus.Running;
        context.Job.FinishedAt = null;
        context.Job.ErrorMessage = null;

        context.Project.Status = ProjectStatus.Running;
        context.Project.UpdatedAt = now;

        await _jobs.UpdateAsync(context.Job, cancellationToken);
        await _jobs.ReplaceTasksAsync(context.Job.Id, context.Tasks, cancellationToken);
        await _projects.UpdateAsync(context.Project, cancellationToken);

        var reviewText = context.Stage.NeedsReview
            ? "The edited output is waiting for checkpoint review."
            : "The edited output was saved and downstream steps are ready to recompute.";
        return new StructuredOutputEditResponse(
            context.Task.Id,
            context.Task.JobId,
            context.Task.ProjectId,
            context.Task.SkillName,
            context.Task.Status.ToString().ToLowerInvariant(),
            resetCount,
            $"Stage 21 edits saved for {context.Task.SkillName}: {string.Join(", ", editedFields.Distinct(StringComparer.OrdinalIgnoreCase))}. {reviewText}");
    }

    private static void EnsureRequiredFields(JsonObject data, string skillName)
    {
        switch (skillName)
        {
            case "character_bible":
                RequireArray(data, "characters");
                RequireArray(data, "relationship_notes");
                RequireArray(data, "continuity_rules");
                break;
            case "style_bible":
                RequireString(data, "style_name");
                RequireObject(data, "visual_style");
                RequireArray(data, "color_palette");
                RequireArray(data, "negative_prompt");
                RequireObject(data, "reusable_prompt_blocks");
                break;
            case "image_prompt_builder":
                RequireArray(data, "image_requests");
                RequireArray(data, "negative_prompt");
                RequireObject(data, "reference_strategy");
                break;
            case "video_prompt_builder":
                RequireArray(data, "video_requests");
                RequireArray(data, "negative_prompt");
                RequireObject(data, "source_asset_manifest");
                break;
        }
    }

    private static void UpdateReviewMetadata(
        JsonObject data,
        string skillName,
        IReadOnlyList<string> editedFields,
        string? notes,
        bool needsReview,
        DateTimeOffset now)
    {
        var normalizedNotes = NormalizeNotes(notes);
        var review = data["review"] as JsonObject ?? new JsonObject();
        data["review"] = review;
        review["status"] = needsReview ? "stage21_edited_ready_for_review" : "stage21_edited_ready_for_recompute";
        review["last_stage21_notes"] = normalizedNotes;
        review["last_stage21_operation"] = "save_structured_output_edits";
        review["stage21_editable_fields"] = StringArray(EditableFields[skillName].ToArray());

        var checkpoint = data["checkpoint"] as JsonObject ?? new JsonObject();
        data["checkpoint"] = checkpoint;
        checkpoint["required"] = needsReview;
        if (needsReview)
        {
            checkpoint["policy"] = $"pause_for_{skillName}_review";
            checkpoint["reason"] = "Stage 21 edited structured output must be reviewed before downstream recompute.";
        }

        data["stage21_edit_summary"] = new JsonObject
        {
            ["operation"] = "save_structured_output_edits",
            ["skill_name"] = skillName,
            ["notes"] = normalizedNotes,
            ["updated_at"] = now.ToString("O", CultureInfo.InvariantCulture),
            ["model_provider"] = "none",
            ["media_generated"] = false,
            ["media_read"] = false,
            ["ffmpeg_invoked"] = false,
            ["downstream_recompute_required"] = true,
            ["edited_fields"] = StringArray(editedFields.Distinct(StringComparer.OrdinalIgnoreCase).ToArray())
        };
    }

    private static string NormalizeFieldName(string path)
    {
        var value = path.Trim().Trim('/').Trim();
        if (value.Length == 0 || value.Contains('/') || value.Contains('.'))
        {
            throw new StructuredOutputEditValidationException($"Editable field path '{path}' must be a top-level data field.");
        }

        return value;
    }

    private static void RequireString(JsonObject data, string propertyName)
    {
        if (ReadString(data, propertyName).Length == 0)
        {
            throw new StructuredOutputEditValidationException($"The edited output is missing {propertyName}.");
        }
    }

    private static JsonArray RequireArray(JsonObject data, string propertyName)
    {
        return data[propertyName] as JsonArray
            ?? throw new StructuredOutputEditValidationException($"The edited output is missing {propertyName}.");
    }

    private static JsonObject RequireObject(JsonObject data, string propertyName)
    {
        return data[propertyName] as JsonObject
            ?? throw new StructuredOutputEditValidationException($"The edited output is missing {propertyName}.");
    }

    private static JsonArray StringArray(params string[] values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static string ReadString(JsonObject? obj, string propertyName, string fallback = "")
    {
        if (obj is null || obj[propertyName] is null)
        {
            return fallback;
        }

        try
        {
            return obj[propertyName]!.GetValue<string>().Trim();
        }
        catch (InvalidOperationException)
        {
            return obj[propertyName]!.ToJsonString();
        }
    }

    private static string? NormalizeNotes(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record StructuredOutputEditContext(
        ProductionJob Job,
        Project Project,
        GenerationTask Task,
        List<GenerationTask> Tasks,
        JsonObject Envelope,
        JsonObject Data,
        ProductionStageDefinition Stage);
}
