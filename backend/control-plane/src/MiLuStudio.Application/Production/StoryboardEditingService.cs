namespace MiLuStudio.Application.Production;

using CultureInfo = global::System.Globalization.CultureInfo;
using DateTimeOffset = global::System.DateTimeOffset;
using JsonArray = global::System.Text.Json.Nodes.JsonArray;
using JsonNode = global::System.Text.Json.Nodes.JsonNode;
using JsonObject = global::System.Text.Json.Nodes.JsonObject;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class StoryboardEditingService
{
    private readonly IClock _clock;
    private readonly IProductionJobRepository _jobs;
    private readonly IProjectRepository _projects;

    public StoryboardEditingService(
        IClock clock,
        IProductionJobRepository jobs,
        IProjectRepository projects)
    {
        _clock = clock;
        _jobs = jobs;
        _projects = projects;
    }

    public async Task<StoryboardEditResponse?> SaveAsync(
        string taskId,
        StoryboardEditRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Shots.Count == 0)
        {
            throw new StoryboardEditValidationException("At least one storyboard shot is required.");
        }

        var task = await _jobs.GetTaskAsync(taskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var context = await LoadContextAsync(task, cancellationToken);
        var editMap = request.Shots.ToDictionary(
            shot => NormalizeShotId(shot.ShotId),
            StringComparer.OrdinalIgnoreCase);
        var shots = RequireArray(context.Data, "shots");

        foreach (var shotNode in shots.OfType<JsonObject>())
        {
            var shotId = NormalizeShotId(ReadString(shotNode, "shot_id"));
            if (shotId.Length == 0 || !editMap.TryGetValue(shotId, out var edit))
            {
                continue;
            }

            ApplyShotEdit(shotNode, edit, request.Notes, "stage17_user_edited");
        }

        EnsureAllEditableShotsApplied(shots, editMap);
        NormalizeTiming(context.Data);
        SyncStoryboardParts(context.Data, request.Notes);
        UpdateReviewMetadata(context.Data, request.Notes, "save_storyboard_edits", _clock.Now);
        context.Data["rendered_markdown"] = RenderMarkdown(context.Data);

        return await PersistEditedStoryboardAsync(
            context,
            request.Notes,
            "Storyboard edits were saved. Downstream steps now require recompute after review.",
            cancellationToken);
    }

    public async Task<StoryboardEditResponse?> RegenerateShotAsync(
        string taskId,
        string shotId,
        StoryboardShotRegenerateRequest request,
        CancellationToken cancellationToken)
    {
        var notes = NormalizeNotes(request.Notes);
        if (notes is null)
        {
            throw new StoryboardEditValidationException("Single-shot recompute requires edit notes.");
        }

        var task = await _jobs.GetTaskAsync(taskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var context = await LoadContextAsync(task, cancellationToken);
        var normalizedShotId = NormalizeShotId(shotId);
        var shots = RequireArray(context.Data, "shots");
        var targetShot = shots
            .OfType<JsonObject>()
            .FirstOrDefault(shot => string.Equals(
                NormalizeShotId(ReadString(shot, "shot_id")),
                normalizedShotId,
                StringComparison.OrdinalIgnoreCase));

        if (targetShot is null)
        {
            throw new StoryboardEditValidationException($"Shot {shotId} was not found.");
        }

        ApplySingleShotRegeneration(targetShot, notes);
        NormalizeTiming(context.Data);
        SyncStoryboardParts(context.Data, notes);
        UpdateReviewMetadata(context.Data, notes, "regenerate_single_shot", _clock.Now);
        context.Data["rendered_markdown"] = RenderMarkdown(context.Data);

        return await PersistEditedStoryboardAsync(
            context,
            notes,
            $"Shot {shotId} was recomputed locally from notes. Downstream steps now require recompute after review.",
            cancellationToken);
    }

    private async Task<StoryboardEditContext> LoadContextAsync(GenerationTask task, CancellationToken cancellationToken)
    {
        if (!string.Equals(task.SkillName, "storyboard_director", StringComparison.OrdinalIgnoreCase))
        {
            throw new StoryboardEditValidationException("Only storyboard_director outputs can be edited through Stage 17.");
        }

        if (string.IsNullOrWhiteSpace(task.OutputJson))
        {
            throw new StoryboardEditValidationException("The storyboard output has not been generated yet.");
        }

        var job = await _jobs.GetAsync(task.JobId, cancellationToken)
            ?? throw new StoryboardEditValidationException("The storyboard job no longer exists.");
        var project = await _projects.GetAsync(task.ProjectId, cancellationToken)
            ?? throw new StoryboardEditValidationException("The storyboard project no longer exists.");
        var tasks = (await _jobs.ListTasksAsync(task.JobId, cancellationToken)).ToList();
        var envelope = JsonNode.Parse(task.OutputJson)?.AsObject()
            ?? throw new StoryboardEditValidationException("The storyboard output is not an editable JSON envelope.");

        if (envelope["ok"]?.GetValue<bool>() != true)
        {
            throw new StoryboardEditValidationException("Failed storyboard outputs must be regenerated before editing.");
        }

        var data = envelope["data"]?.AsObject()
            ?? throw new StoryboardEditValidationException("The storyboard envelope is missing data.");

        return new StoryboardEditContext(job, project, task, tasks, envelope, data);
    }

    private async Task<StoryboardEditResponse> PersistEditedStoryboardAsync(
        StoryboardEditContext context,
        string? notes,
        string message,
        CancellationToken cancellationToken)
    {
        var now = _clock.Now;
        context.Task.OutputJson = context.Envelope.ToJsonString();
        context.Task.Status = GenerationTaskStatus.Review;
        context.Task.FinishedAt = null;
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

        context.Job.Status = ProductionJobStatus.Paused;
        context.Job.CurrentStage = ProductionStage.StoryboardReadyForReview;
        context.Job.ProgressPercent = ProductionStageCatalog.ProgressFor(ProductionStage.StoryboardReadyForReview);
        context.Job.FinishedAt = null;
        context.Job.ErrorMessage = null;

        context.Project.Status = ProjectStatus.Running;
        context.Project.UpdatedAt = now;

        await _jobs.UpdateAsync(context.Job, cancellationToken);
        await _jobs.ReplaceTasksAsync(context.Job.Id, context.Tasks, cancellationToken);
        await _projects.UpdateAsync(context.Project, cancellationToken);

        return new StoryboardEditResponse(
            context.Task.Id,
            context.Task.JobId,
            context.Task.ProjectId,
            "review",
            resetCount,
            message);
    }

    private static void ApplyShotEdit(JsonObject shot, StoryboardShotEdit edit, string? notes, string editFlag)
    {
        var durationSeconds = ValidateDuration(edit.DurationSeconds);
        shot["duration_seconds"] = durationSeconds;
        shot["scene"] = NormalizeRequiredText(edit.Scene, "Shot scene cannot be empty.");
        shot["visual_action"] = NormalizeRequiredText(edit.VisualAction, "Shot action cannot be empty.");
        shot["shot_size"] = NormalizeRequiredText(edit.ShotSize, "Shot size cannot be empty.");
        shot["sound_note"] = NormalizeText(edit.SoundNote);
        shot["narration"] = NormalizeText(edit.Narration);
        shot["dialogue"] = BuildDialogueArray(edit.Dialogue);

        var camera = shot["camera"] as JsonObject;
        if (camera is null)
        {
            camera = new JsonObject();
            shot["camera"] = camera;
        }

        camera["motion"] = NormalizeRequiredText(edit.CameraMovement, "Camera movement cannot be empty.");

        AppendUniqueText(EnsureArray(shot, "review_flags"), editFlag);
        if (NormalizeNotes(notes) is { } normalizedNotes)
        {
            AppendUniqueText(EnsureArray(shot, "continuity_notes"), $"user_edit_notes: {normalizedNotes}");
        }

        var shotId = ReadString(shot, "shot_id");
        shot["image_prompt_seed"] = AppendPromptAdjustment(ReadString(shot, "image_prompt_seed"), edit.Scene);
        shot["video_prompt_seed"] = AppendPromptAdjustment(ReadString(shot, "video_prompt_seed"), edit.VisualAction);
        shot["stage17_edit"] = new JsonObject
        {
            ["operation"] = editFlag,
            ["shot_id"] = shotId,
            ["updated_fields"] = StringArray(
                "duration_seconds",
                "scene",
                "visual_action",
                "shot_size",
                "camera.motion",
                "dialogue",
                "narration",
                "sound_note")
        };
    }

    private static void ApplySingleShotRegeneration(JsonObject shot, string notes)
    {
        var originalScene = ReadString(shot, "scene");
        var originalAction = ReadString(shot, "visual_action");
        var originalSound = ReadString(shot, "sound_note");

        shot["scene"] = MergeUserInstruction(originalScene, notes);
        shot["visual_action"] = MergeUserInstruction(originalAction, notes);
        shot["sound_note"] = string.IsNullOrWhiteSpace(originalSound)
            ? $"sound_recompute_notes: {notes}"
            : MergeUserInstruction(originalSound, notes);

        AppendUniqueText(EnsureArray(shot, "review_flags"), "stage17_single_shot_regenerated");
        AppendUniqueText(EnsureArray(shot, "continuity_notes"), $"single_shot_recompute_notes: {notes}");
        shot["image_prompt_seed"] = AppendPromptAdjustment(ReadString(shot, "image_prompt_seed"), notes);
        shot["video_prompt_seed"] = AppendPromptAdjustment(ReadString(shot, "video_prompt_seed"), notes);
        shot["stage17_edit"] = new JsonObject
        {
            ["operation"] = "regenerate_single_shot",
            ["notes"] = notes,
            ["model_provider"] = "none",
            ["media_generated"] = false
        };
    }

    private static void EnsureAllEditableShotsApplied(JsonArray shots, IReadOnlyDictionary<string, StoryboardShotEdit> editMap)
    {
        var existing = shots
            .OfType<JsonObject>()
            .Select(shot => NormalizeShotId(ReadString(shot, "shot_id")))
            .Where(shotId => shotId.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = editMap.Keys.Where(shotId => !existing.Contains(shotId)).ToList();
        if (missing.Count > 0)
        {
            throw new StoryboardEditValidationException($"Storyboard edits include unknown shots: {string.Join(", ", missing)}.");
        }
    }

    private static void NormalizeTiming(JsonObject data)
    {
        var shots = RequireArray(data, "shots");
        var target = ReadInt(data, "target_duration_seconds");
        var currentSecond = 0;
        var minDuration = int.MaxValue;
        var maxDuration = 0;
        var index = 1;

        foreach (var shot in shots.OfType<JsonObject>())
        {
            var duration = ValidateDuration(ReadInt(shot, "duration_seconds"));
            shot["shot_index"] = index;
            shot["start_second"] = currentSecond;
            shot["duration_seconds"] = duration;
            currentSecond += duration;
            minDuration = Math.Min(minDuration, duration);
            maxDuration = Math.Max(maxDuration, duration);
            index++;
        }

        if (shots.Count < 6 || shots.Count > 12)
        {
            throw new StoryboardEditValidationException("The MVP storyboard must keep 6 to 12 shots.");
        }

        if (target > 0 && Math.Abs(currentSecond - target) > 1)
        {
            throw new StoryboardEditValidationException($"Total shot duration must stay within 1 second of target. Current={currentSecond}, target={target}.");
        }

        data["storyboard_summary"] = $"Edited storyboard: {shots.Count} shots, {currentSecond}s total.";
        data["timing_summary"] = new JsonObject
        {
            ["target_duration_seconds"] = target,
            ["total_shot_seconds"] = currentSecond,
            ["shot_count"] = shots.Count,
            ["min_shot_seconds"] = minDuration == int.MaxValue ? 0 : minDuration,
            ["max_shot_seconds"] = maxDuration,
            ["within_tolerance"] = target <= 0 || Math.Abs(currentSecond - target) <= 1
        };

        var overview = data["film_overview"] as JsonObject;
        if (overview is null)
        {
            overview = new JsonObject();
            data["film_overview"] = overview;
        }

        overview["total_duration_seconds"] = currentSecond;
        overview["target_duration_seconds"] = target;
        overview["shot_count"] = shots.Count;
    }

    private static void SyncStoryboardParts(JsonObject data, string? notes)
    {
        var shotsById = RequireArray(data, "shots")
            .OfType<JsonObject>()
            .Where(shot => ReadString(shot, "shot_id").Length > 0)
            .ToDictionary(shot => ReadString(shot, "shot_id"), StringComparer.OrdinalIgnoreCase);
        var parts = data["storyboard_parts"] as JsonArray;
        if (parts is null)
        {
            return;
        }

        foreach (var part in parts.OfType<JsonObject>())
        {
            var formattedShots = part["shots"] as JsonArray;
            if (formattedShots is null)
            {
                continue;
            }

            var partStart = int.MaxValue;
            var partEnd = 0;
            foreach (var formatted in formattedShots.OfType<JsonObject>())
            {
                var sourceShotId = ReadString(formatted, "source_shot_id");
                if (!shotsById.TryGetValue(sourceShotId, out var source))
                {
                    continue;
                }

                ApplyFormattedShot(formatted, source, notes);
                var start = ReadInt(source, "start_second");
                var end = start + ReadInt(source, "duration_seconds");
                partStart = Math.Min(partStart, start);
                partEnd = Math.Max(partEnd, end);
            }

            if (partStart != int.MaxValue)
            {
                part["start_second"] = partStart;
                part["end_second"] = partEnd;
                part["duration_seconds"] = partEnd - partStart;
            }
        }
    }

    private static void ApplyFormattedShot(JsonObject formatted, JsonObject source, string? notes)
    {
        var duration = ReadInt(source, "duration_seconds");
        formatted["duration_seconds"] = duration;
        formatted["shot_label"] = $"Shot {ReadInt(source, "shot_index")}";
        formatted["environment_description"] = ReadString(source, "scene");
        formatted["time_slice"] = $"0.0-{duration.ToString("0.0", CultureInfo.InvariantCulture)}s: {ReadString(source, "visual_action")} {FormatDialogue(source)} {ReadString(source, "narration")}".Trim();
        formatted["shot_size"] = ReadString(source, "shot_size");
        formatted["camera_movement"] = ReadString(source["camera"] as JsonObject, "motion", "Locked camera");
        formatted["sound_effect"] = ReadString(source, "sound_note", "Keep clear ambience and dialogue.");
        formatted["dialogue"] = FormatDialogue(source);
        formatted["narration"] = ReadString(source, "narration");
        if (NormalizeNotes(notes) is { } normalizedNotes)
        {
            formatted["stage17_notes"] = normalizedNotes;
        }
    }

    private static void UpdateReviewMetadata(JsonObject data, string? notes, string operation, DateTimeOffset now)
    {
        var normalizedNotes = NormalizeNotes(notes);

        var review = data["review"] as JsonObject;
        if (review is null)
        {
            review = new JsonObject();
            data["review"] = review;
        }

        review["status"] = "edited_ready_for_review";
        review["last_stage17_operation"] = operation;
        review["last_stage17_notes"] = normalizedNotes;

        var checkpoint = data["checkpoint"] as JsonObject;
        if (checkpoint is null)
        {
            checkpoint = new JsonObject();
            data["checkpoint"] = checkpoint;
        }

        checkpoint["required"] = true;
        checkpoint["policy"] = "pause_for_storyboard_review";
        checkpoint["reason"] = "Stage 17 edited storyboard must be reviewed before downstream recompute.";

        data["stage17_edit_summary"] = new JsonObject
        {
            ["operation"] = operation,
            ["notes"] = normalizedNotes,
            ["updated_at"] = now.ToString("O", CultureInfo.InvariantCulture),
            ["model_provider"] = "none",
            ["media_generated"] = false,
            ["downstream_recompute_required"] = true
        };

        var checks = new JsonArray();
        checks.Add(new JsonObject
        {
            ["name"] = "storyboard_edit",
            ["status"] = "passed",
            ["detail"] = "User edits were persisted into the storyboard_director envelope."
        });
        checks.Add(new JsonObject
        {
            ["name"] = "downstream_recompute",
            ["status"] = "required",
            ["detail"] = "Image, video, voice, subtitle, edit, quality, and export steps were reset for recompute after approval."
        });
        checks.Add(new JsonObject
        {
            ["name"] = "media_boundary",
            ["status"] = "passed",
            ["detail"] = "No real provider, media file read, FFmpeg call, or final media artifact was produced."
        });

        data["validation_report"] = new JsonObject
        {
            ["profile"] = "cinematic_md_v1",
            ["strict_md_ready"] = false,
            ["checks"] = checks,
            ["deferred_requirements"] = StringArray(
                "real TextProvider-backed long storyboard rewrite",
                "dialogue fidelity validation",
                "50-55 shot production-scale storyboard editing")
        };
    }

    private static string RenderMarkdown(JsonObject data)
    {
        var overview = data["film_overview"] as JsonObject;
        var lines = new List<string>
        {
            ReadString(overview, "episode_label", "Episode"),
            string.Empty,
            "Film overview",
            $"Theme: {ReadString(overview, "theme", ReadString(data, "title", "Untitled"))}",
            $"Total duration: {ReadInt(overview, "total_duration_seconds")}s (target {ReadInt(overview, "target_duration_seconds")}s)",
            $"Shot count: {ReadInt(overview, "shot_count")}",
            $"Style tone: {ReadString(overview, "style_tone")}",
            $"Camera setup: {ReadString(overview, "camera_setup")}",
            string.Empty
        };

        if (data["storyboard_parts"] is JsonArray parts)
        {
            foreach (var part in parts.OfType<JsonObject>())
            {
                lines.Add(ReadString(part, "title", "Storyboard part"));
                lines.Add($"Time/weather/light: {ReadString(part, "time_weather_light")}");
                lines.Add($"Camera setup: {ReadString(part, "camera_setup")}");
                lines.Add($"Cast and props: {ReadString(part, "cast_and_props")}");
                lines.Add($"Blocking: {ReadString(part, "absolute_blocking")}");
                lines.Add($"Style: {ReadString(part, "style")}");
                lines.Add(string.Empty);

                if (part["shots"] is not JsonArray partShots)
                {
                    continue;
                }

                foreach (var shot in partShots.OfType<JsonObject>())
                {
                    lines.Add(ReadString(shot, "shot_label", "Shot"));
                    lines.Add($"Duration: {ReadInt(shot, "duration_seconds").ToString("0.0", CultureInfo.InvariantCulture)}s");
                    lines.Add($"Environment: {ReadString(shot, "environment_description")}");
                    lines.Add($"Time slice: {ReadString(shot, "time_slice")}");
                    lines.Add($"Shot size: {ReadString(shot, "shot_size")}");
                    lines.Add($"Camera movement: {ReadString(shot, "camera_movement")}");
                    lines.Add($"Sound: {ReadString(shot, "sound_effect")}");
                    lines.Add($"Music: {ReadString(shot, "background_music")}");
                    lines.Add(string.Empty);
                }
            }
        }

        return string.Join('\n', lines).Trim();
    }

    private static JsonArray RequireArray(JsonObject data, string propertyName)
    {
        return data[propertyName] as JsonArray
            ?? throw new StoryboardEditValidationException($"The storyboard output is missing {propertyName}.");
    }

    private static JsonArray EnsureArray(JsonObject parent, string propertyName)
    {
        var existing = parent[propertyName] as JsonArray;
        if (existing is not null)
        {
            return existing;
        }

        var array = new JsonArray();
        parent[propertyName] = array;
        return array;
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

    private static int ValidateDuration(int durationSeconds)
    {
        if (durationSeconds < 1 || durationSeconds > 15)
        {
            throw new StoryboardEditValidationException("Single-shot duration must be between 1 and 15 seconds.");
        }

        return durationSeconds;
    }

    private static string NormalizeRequiredText(string value, string message)
    {
        var text = NormalizeText(value);
        if (text.Length == 0)
        {
            throw new StoryboardEditValidationException(message);
        }

        return text;
    }

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string? NormalizeNotes(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeShotId(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static JsonArray BuildDialogueArray(string value)
    {
        var text = NormalizeText(value);
        if (text.Length == 0)
        {
            return new JsonArray();
        }

        var speaker = "Dialogue";
        var line = text;
        var separatorIndex = text.IndexOf(':', StringComparison.Ordinal);
        if (separatorIndex > 0 && separatorIndex < text.Length - 1)
        {
            speaker = text[..separatorIndex].Trim();
            line = text[(separatorIndex + 1)..].Trim().Trim('"');
        }

        var array = new JsonArray();
        array.Add(new JsonObject
        {
            ["speaker"] = speaker,
            ["line"] = line,
            ["delivery"] = "Use the edited storyboard rhythm."
        });
        return array;
    }

    private static string FormatDialogue(JsonObject shot)
    {
        if (shot["dialogue"] is not JsonArray dialogueLines)
        {
            return string.Empty;
        }

        return string.Join(
            "; ",
            dialogueLines
                .OfType<JsonObject>()
                .Select(line =>
                {
                    var speaker = ReadString(line, "speaker");
                    var text = ReadString(line, "line");
                    return speaker.Length > 0 && text.Length > 0 ? $"{speaker}: \"{text}\"" : text;
                })
                .Where(value => value.Length > 0));
    }

    private static string AppendPromptAdjustment(string prompt, string adjustment)
    {
        var normalizedAdjustment = NormalizeText(adjustment);
        if (normalizedAdjustment.Length == 0)
        {
            return prompt;
        }

        var suffix = $"stage17_adjustment={normalizedAdjustment}";
        return prompt.Contains(suffix, StringComparison.OrdinalIgnoreCase)
            ? prompt
            : $"{prompt} | {suffix}".Trim(' ', '|');
    }

    private static string MergeUserInstruction(string original, string notes)
    {
        return string.IsNullOrWhiteSpace(original)
            ? $"recomputed_from_notes: {notes}"
            : $"{original}; recomputed_from_notes: {notes}";
    }

    private static void AppendUniqueText(JsonArray array, string value)
    {
        if (array.Any(node => string.Equals(node?.GetValue<string>(), value, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        array.Add(value);
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

    private static int ReadInt(JsonObject? obj, string propertyName)
    {
        if (obj is null || obj[propertyName] is null)
        {
            return 0;
        }

        try
        {
            return obj[propertyName]!.GetValue<int>();
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    private sealed record StoryboardEditContext(
        ProductionJob Job,
        Project Project,
        GenerationTask Task,
        List<GenerationTask> Tasks,
        JsonObject Envelope,
        JsonObject Data);
}
