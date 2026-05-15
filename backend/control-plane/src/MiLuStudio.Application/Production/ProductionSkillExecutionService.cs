namespace MiLuStudio.Application.Production;

using JsonArray = global::System.Text.Json.Nodes.JsonArray;
using JsonDocument = global::System.Text.Json.JsonDocument;
using JsonException = global::System.Text.Json.JsonException;
using JsonNode = global::System.Text.Json.Nodes.JsonNode;
using JsonObject = global::System.Text.Json.Nodes.JsonObject;
using JsonSerializer = global::System.Text.Json.JsonSerializer;
using JsonSerializerDefaults = global::System.Text.Json.JsonSerializerDefaults;
using JsonSerializerOptions = global::System.Text.Json.JsonSerializerOptions;
using JsonValue = global::System.Text.Json.Nodes.JsonValue;
using JsonValueKind = global::System.Text.Json.JsonValueKind;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class ProductionSkillExecutionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IClock _clock;
    private readonly IAssetRepository _assets;
    private readonly IProductionJobRepository _jobs;
    private readonly IProductionSkillRunner _runner;
    private readonly IProjectRepository _projects;
    private readonly SkillEnvelopePersistenceService _persistence;

    public ProductionSkillExecutionService(
        IClock clock,
        IAssetRepository assets,
        IProductionJobRepository jobs,
        IProductionSkillRunner runner,
        IProjectRepository projects,
        SkillEnvelopePersistenceService persistence)
    {
        _clock = clock;
        _assets = assets;
        _jobs = jobs;
        _runner = runner;
        _projects = projects;
        _persistence = persistence;
    }

    public async Task<ProductionSkillExecutionResult> ExecuteAsync(
        GenerationTask claimedTask,
        CancellationToken cancellationToken)
    {
        var stage = ProductionStageCatalog.FindBySkill(claimedTask.SkillName);
        if (stage is null)
        {
            return await PersistFailureAsync(
                claimedTask,
                "unknown_skill",
                $"No production stage is registered for skill '{claimedTask.SkillName}'.",
                cancellationToken);
        }

        var job = await _jobs.GetAsync(claimedTask.JobId, cancellationToken);
        if (job is null)
        {
            return await PersistFailureAsync(
                claimedTask,
                "missing_job",
                $"Production job '{claimedTask.JobId}' does not exist.",
                cancellationToken);
        }

        await MarkJobRunningAsync(job, stage, cancellationToken);

        try
        {
            var tasks = await _jobs.ListTasksAsync(claimedTask.JobId, cancellationToken);
            var task = tasks.FirstOrDefault(task => task.Id == claimedTask.Id) ?? claimedTask;
            var inputJson = await BuildInputJsonAsync(task, tasks, cancellationToken);

            task.InputJson = inputJson;
            task.LastHeartbeatAt = _clock.Now;
            await _jobs.UpdateTaskAsync(task, cancellationToken);

            var result = await _runner.RunAsync(task.SkillName, inputJson, cancellationToken);
            var response = await _persistence.PersistAsync(
                task.Id,
                new PersistSkillEnvelopeRequest(
                    result.OutputJson,
                    "skill_envelope",
                    "deterministic-python-skill",
                    "none",
                    "skill_envelope",
                    1,
                    ExtractRuntimeCost(result.OutputJson),
                    0,
                    stage.NeedsReview),
                cancellationToken);

            if (response is null)
            {
                return new ProductionSkillExecutionResult(task.Id, task.SkillName, "missing_task", "Task disappeared before output persistence.");
            }

            await ApplyPersistedTaskStateAsync(job, stage, response.Status, result.OutputJson, cancellationToken);

            return new ProductionSkillExecutionResult(task.Id, task.SkillName, response.Status, BuildResultMessage(task.SkillName, response.Status));
        }
        catch (Exception error) when (!cancellationToken.IsCancellationRequested)
        {
            var failure = await PersistFailureAsync(
                claimedTask,
                "worker_skill_execution_failed",
                $"{error.GetType().Name}: {error.Message}",
                cancellationToken);

            var failedJob = await _jobs.GetAsync(claimedTask.JobId, cancellationToken);
            if (failedJob is not null)
            {
                failedJob.Status = ProductionJobStatus.Failed;
                failedJob.CurrentStage = ProductionStage.FailedRetryable;
                failedJob.ErrorMessage = failure.Message;
                await _jobs.UpdateAsync(failedJob, cancellationToken);
            }

            return failure;
        }
    }

    private async Task<string> BuildInputJsonAsync(
        GenerationTask task,
        IReadOnlyList<GenerationTask> tasks,
        CancellationToken cancellationToken)
    {
        var payload = task.SkillName switch
        {
            "story_intake" => await BuildStoryIntakePayloadAsync(task.ProjectId, cancellationToken),
            "plot_adaptation" => BuildEnvelopePayload(tasks, ["story_intake"], extra =>
            {
                extra["adaptation_preferences"] = new JsonObject
                {
                    ["narrative_pov"] = "protagonist",
                    ["ending_style"] = "open_hook"
                };
            }),
            "episode_writer" => BuildEnvelopePayload(tasks, ["plot_adaptation"], extra =>
            {
                extra["writing_preferences"] = new JsonObject
                {
                    ["dialogue_density"] = "balanced"
                };
            }),
            "character_bible" => BuildEnvelopePayload(tasks, ["episode_writer"], extra =>
            {
                extra["character_preferences"] = new JsonObject
                {
                    ["locked_character_names"] = new JsonArray()
                };
            }),
            "style_bible" => BuildEnvelopePayload(tasks, ["episode_writer", "character_bible"]),
            "storyboard_director" => BuildEnvelopePayload(tasks, ["episode_writer", "character_bible", "style_bible"]),
            "image_prompt_builder" => await BuildEnvelopePayloadWithAssetAnalysisAsync(
                task.ProjectId,
                tasks,
                ["storyboard_director", "character_bible", "style_bible"],
                includeImageReferences: true,
                includeVideoReferences: false,
                cancellationToken),
            "image_generation" => BuildEnvelopePayload(tasks, ["image_prompt_builder"]),
            "video_prompt_builder" => await BuildEnvelopePayloadWithAssetAnalysisAsync(
                task.ProjectId,
                tasks,
                ["storyboard_director", "image_prompt_builder", "image_generation"],
                includeImageReferences: true,
                includeVideoReferences: true,
                cancellationToken),
            "video_generation" => BuildEnvelopePayload(tasks, ["video_prompt_builder"]),
            "voice_casting" => BuildEnvelopePayload(tasks, ["episode_writer", "storyboard_director"]),
            "subtitle_generator" => BuildEnvelopePayload(tasks, ["episode_writer", "storyboard_director", "voice_casting"]),
            "auto_editor" => BuildEnvelopePayload(tasks, ["storyboard_director", "video_generation", "voice_casting", "subtitle_generator"]),
            "quality_checker" => BuildEnvelopePayload(tasks, ["character_bible", "style_bible", "storyboard_director", "video_generation", "voice_casting", "subtitle_generator", "auto_editor"]),
            "export_packager" => BuildEnvelopePayload(tasks, ["auto_editor", "quality_checker", "subtitle_generator", "video_generation"]),
            _ => throw new InvalidOperationException($"Skill '{task.SkillName}' is not supported by the Stage 13 worker input builder.")
        };

        return payload.ToJsonString(JsonOptions);
    }

    private async Task<JsonObject> BuildEnvelopePayloadWithAssetAnalysisAsync(
        string projectId,
        IReadOnlyList<GenerationTask> tasks,
        IReadOnlyList<string> skillNames,
        bool includeImageReferences,
        bool includeVideoReferences,
        CancellationToken cancellationToken)
    {
        var payload = BuildEnvelopePayload(tasks, skillNames);
        payload["asset_analysis"] = await BuildReferenceAssetAnalysisPayloadAsync(
            projectId,
            includeImageReferences,
            includeVideoReferences,
            cancellationToken);

        return payload;
    }

    private async Task<JsonObject> BuildStoryIntakePayloadAsync(string projectId, CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(projectId, cancellationToken)
            ?? throw new InvalidOperationException($"Project '{projectId}' does not exist.");
        var storyInput = await _projects.GetStoryInputAsync(projectId, cancellationToken)
            ?? throw new InvalidOperationException($"Project '{projectId}' has no story input.");
        var storyText = await ResolveStoryTextForProductionAsync(projectId, storyInput.OriginalText, cancellationToken);

        return new JsonObject
        {
            ["project_id"] = project.Id,
            ["story_text"] = storyText,
            ["language"] = storyInput.Language,
            ["target_duration_seconds"] = project.TargetDurationSeconds,
            ["aspect_ratio"] = project.AspectRatio,
            ["style_preset"] = project.StylePreset,
            ["mode"] = project.Mode == ProjectMode.Fast ? "fast" : "director"
        };
    }

    private async Task<string> ResolveStoryTextForProductionAsync(
        string projectId,
        string fallbackStoryText,
        CancellationToken cancellationToken)
    {
        var assets = await _assets.ListAssetsByProjectAsync(projectId, cancellationToken);
        foreach (var asset in assets
            .Where(asset => string.Equals(asset.Kind, "story_text", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(asset => asset.CreatedAt))
        {
            var candidate = ExtractStoryTextCandidate(asset.MetadataJson);
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate.Trim();
            }
        }

        return fallbackStoryText;
    }

    private static string? ExtractStoryTextCandidate(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            var metadata = JsonNode.Parse(metadataJson) as JsonObject;
            return ExtractProductionInputStoryText(metadata) ?? ExtractChunkContentStoryText(metadata);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ExtractProductionInputStoryText(JsonObject? metadata)
    {
        var productionInput = GetObject(metadata, "productionInput");
        if (GetBool(productionInput, "usableAsStoryCandidate") != true)
        {
            return null;
        }

        return GetString(productionInput, "storyTextCandidate");
    }

    private static string? ExtractChunkContentStoryText(JsonObject? metadata)
    {
        var technical = GetObject(metadata, "technical");
        var chunkManifest = GetObject(technical, "chunkManifest");
        if (!string.Equals(GetString(chunkManifest, "status"), "ok", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (GetNode(chunkManifest, "chunks") is not JsonArray chunks)
        {
            return null;
        }

        var parts = chunks
            .OfType<JsonObject>()
            .Select(chunk => GetString(chunk, "content") ?? GetString(chunk, "text"))
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();

        return parts.Count == 0 ? null : string.Join(Environment.NewLine, parts);
    }

    private async Task<JsonObject> BuildReferenceAssetAnalysisPayloadAsync(
        string projectId,
        bool includeImageReferences,
        bool includeVideoReferences,
        CancellationToken cancellationToken)
    {
        var imageReferences = new JsonArray();
        var videoReferences = new JsonArray();
        var assets = await _assets.ListAssetsByProjectAsync(projectId, cancellationToken);

        foreach (var asset in assets.OrderByDescending(asset => asset.CreatedAt))
        {
            if (includeImageReferences && string.Equals(asset.Kind, "image_reference", StringComparison.OrdinalIgnoreCase))
            {
                imageReferences.Add(BuildReferenceAssetCandidate(asset));
            }

            if (includeVideoReferences && string.Equals(asset.Kind, "video_reference", StringComparison.OrdinalIgnoreCase))
            {
                videoReferences.Add(BuildReferenceAssetCandidate(asset));
            }
        }

        return new JsonObject
        {
            ["schema_version"] = "stage23c_reference_asset_analysis_v1",
            ["source"] = "control_api_asset_metadata",
            ["analysis_endpoint_contract"] = "/api/projects/{projectId}/assets/{assetId}/analysis",
            ["media_access_policy"] = "backend_adapter_only",
            ["ui_electron_file_access"] = false,
            ["generation_payload_sent"] = false,
            ["model_provider_used"] = false,
            ["image_reference_count"] = imageReferences.Count,
            ["video_reference_count"] = videoReferences.Count,
            ["image_references"] = imageReferences,
            ["video_references"] = videoReferences
        };
    }

    private static JsonObject BuildReferenceAssetCandidate(Asset asset)
    {
        var metadata = ParseMetadata(asset.MetadataJson);
        var technical = GetObject(metadata, "technical");
        var parse = GetObject(metadata, "parse");
        var productionInput = GetObject(metadata, "productionInput");
        var ocr = GetObject(technical, "ocr");
        var derivativeSummary = BuildReferenceDerivativeSummary(metadata, technical);
        var frameExtraction = BuildFrameExtractionSummary(technical);

        return new JsonObject
        {
            ["asset_id"] = asset.Id,
            ["project_id"] = asset.ProjectId,
            ["kind"] = asset.Kind,
            ["original_file_name"] = GetString(metadata, "originalFileName"),
            ["mime_type"] = asset.MimeType,
            ["file_size"] = asset.FileSize,
            ["sha256"] = asset.Sha256,
            ["created_at"] = asset.CreatedAt.ToUniversalTime().ToString("O"),
            ["analysis_endpoint"] = $"/api/projects/{asset.ProjectId}/assets/{asset.Id}/analysis",
            ["source"] = GetString(productionInput, "source") ?? "asset_metadata_json",
            ["parse_status"] = GetString(parse, "Status") ?? GetString(parse, "status") ?? "unknown",
            ["extracted_text_length"] = GetInt(parse, "extractedTextLength") ?? GetInt(ocr, "extractedTextLength") ?? 0,
            ["usable_as_image_reference"] = string.Equals(asset.Kind, "image_reference", StringComparison.OrdinalIgnoreCase),
            ["usable_as_video_reference"] = string.Equals(asset.Kind, "video_reference", StringComparison.OrdinalIgnoreCase),
            ["derivative_count"] = GetInt(derivativeSummary, "count") ?? 0,
            ["derivative_kinds"] = GetNode(derivativeSummary, "kinds")?.DeepClone(),
            ["has_thumbnail"] = HasDerivativeKind(derivativeSummary, "thumbnail"),
            ["has_image_preview"] = HasDerivativeKind(derivativeSummary, "image_preview"),
            ["has_video_frames"] = HasDerivativeKind(derivativeSummary, "video_frame"),
            ["has_video_review_proxy"] = HasDerivativeKind(derivativeSummary, "video_review_proxy"),
            ["video_frame_count"] = GetInt(frameExtraction, "actual_frame_count") ?? 0,
            ["probe_summary"] = BuildProbeSummary(technical),
            ["ocr_summary"] = BuildOcrSummary(ocr),
            ["frame_extraction"] = frameExtraction,
            ["media_access_policy"] = "backend_adapter_only",
            ["local_paths_exposed"] = false,
            ["ui_electron_file_access"] = false,
            ["generation_payload_sent"] = false,
            ["model_provider_used"] = false
        };
    }

    private static JsonObject? ParseMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(metadataJson) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static JsonObject BuildReferenceDerivativeSummary(JsonObject? metadata, JsonObject? technical)
    {
        var kinds = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var count = 0;

        if (GetNode(metadata, "derivatives") is JsonArray derivatives)
        {
            count = derivatives.Count;
        }

        if (GetNode(technical, "derivativeDetails") is JsonArray details)
        {
            count = Math.Max(count, details.Count);
            foreach (var detail in details.OfType<JsonObject>())
            {
                var kind = GetString(detail, "kind");
                if (!string.IsNullOrWhiteSpace(kind))
                {
                    kinds.Add(kind);
                }
            }
        }

        return new JsonObject
        {
            ["count"] = count,
            ["kinds"] = StringArray(kinds),
            ["access_policy"] = "backend_adapter_only",
            ["local_paths_exposed"] = false
        };
    }

    private static bool HasDerivativeKind(JsonObject derivativeSummary, string kind)
    {
        return GetNode(derivativeSummary, "kinds") is JsonArray kinds &&
            kinds.OfType<JsonValue>().Any(value => value.TryGetValue<string>(out var candidate) &&
                string.Equals(candidate, kind, StringComparison.OrdinalIgnoreCase));
    }

    private static JsonObject BuildProbeSummary(JsonObject? technical)
    {
        var probe = GetObject(technical, "probeSummary");
        if (probe is null)
        {
            return new JsonObject { ["status"] = "unavailable" };
        }

        return new JsonObject
        {
            ["status"] = GetString(probe, "status") ?? "unknown",
            ["format"] = GetNode(probe, "format")?.DeepClone(),
            ["streams"] = GetNode(probe, "streams")?.DeepClone()
        };
    }

    private static JsonObject BuildOcrSummary(JsonObject? ocr)
    {
        return new JsonObject
        {
            ["status"] = GetString(ocr, "status") ?? "not_recorded",
            ["candidate"] = GetBool(ocr, "candidate") ?? false,
            ["invoked"] = GetBool(ocr, "invoked") ?? false,
            ["language"] = GetString(ocr, "language"),
            ["extracted_text_length"] = GetInt(ocr, "extractedTextLength") ?? 0,
            ["ui_electron_file_access"] = GetBool(ocr, "uiElectronFileAccess") ?? false,
            ["model_provider_used"] = GetBool(ocr, "modelProviderUsed") ?? false
        };
    }

    private static JsonObject? BuildFrameExtractionSummary(JsonObject? technical)
    {
        var frameExtraction = GetObject(technical, "frameExtraction");
        if (frameExtraction is null)
        {
            return null;
        }

        return new JsonObject
        {
            ["status"] = GetString(frameExtraction, "status") ?? "not_recorded",
            ["sampling"] = GetString(frameExtraction, "sampling"),
            ["target_frame_count"] = GetInt(frameExtraction, "targetFrameCount") ?? 0,
            ["actual_frame_count"] = GetInt(frameExtraction, "actualFrameCount") ?? 0,
            ["interval_seconds"] = GetInt(frameExtraction, "intervalSeconds") ?? 0,
            ["duration_seconds"] = GetDouble(frameExtraction, "durationSeconds"),
            ["local_frame_directory_exposed"] = false
        };
    }

    private static JsonNode? GetNode(JsonObject? source, string property)
    {
        if (source is null)
        {
            return null;
        }

        return source.TryGetPropertyValue(property, out var value) ? value : null;
    }

    private static JsonObject? GetObject(JsonObject? source, string property)
    {
        return GetNode(source, property) as JsonObject;
    }

    private static string? GetString(JsonObject? source, string property)
    {
        if (GetNode(source, property) is not JsonValue value)
        {
            return null;
        }

        return value.TryGetValue<string>(out var result) ? result : null;
    }

    private static bool? GetBool(JsonObject? source, string property)
    {
        if (GetNode(source, property) is not JsonValue value)
        {
            return null;
        }

        return value.TryGetValue<bool>(out var result) ? result : null;
    }

    private static int? GetInt(JsonObject? source, string property)
    {
        if (GetNode(source, property) is not JsonValue value)
        {
            return null;
        }

        return value.TryGetValue<int>(out var intResult)
            ? intResult
            : value.TryGetValue<long>(out var longResult)
                ? checked((int)longResult)
                : null;
    }

    private static double? GetDouble(JsonObject? source, string property)
    {
        if (GetNode(source, property) is not JsonValue value)
        {
            return null;
        }

        return value.TryGetValue<double>(out var result) ? result : null;
    }

    private static JsonArray StringArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static JsonObject BuildEnvelopePayload(
        IReadOnlyList<GenerationTask> tasks,
        IReadOnlyList<string> skillNames,
        Action<JsonObject>? configure = null)
    {
        var payload = new JsonObject();
        foreach (var skillName in skillNames)
        {
            payload[skillName] = ParseRequiredEnvelope(tasks, skillName);
        }

        configure?.Invoke(payload);
        return payload;
    }

    private static JsonNode ParseRequiredEnvelope(IReadOnlyList<GenerationTask> tasks, string skillName)
    {
        var source = tasks.FirstOrDefault(task => string.Equals(task.SkillName, skillName, StringComparison.OrdinalIgnoreCase));
        if (source is null)
        {
            throw new InvalidOperationException($"Previous skill '{skillName}' is missing.");
        }

        if (source.Status != GenerationTaskStatus.Completed || string.IsNullOrWhiteSpace(source.OutputJson))
        {
            throw new InvalidOperationException($"Previous skill '{skillName}' has no completed output envelope.");
        }

        return JsonNode.Parse(source.OutputJson)
            ?? throw new InvalidOperationException($"Previous skill '{skillName}' output envelope is empty.");
    }

    private async Task MarkJobRunningAsync(
        ProductionJob job,
        ProductionStageDefinition stage,
        CancellationToken cancellationToken)
    {
        job.Status = ProductionJobStatus.Running;
        job.CurrentStage = stage.Stage;
        job.ProgressPercent = ProductionStageCatalog.ProgressFor(stage.Stage);
        job.ErrorMessage = null;
        await _jobs.UpdateAsync(job, cancellationToken);
    }

    private async Task ApplyPersistedTaskStateAsync(
        ProductionJob job,
        ProductionStageDefinition stage,
        string taskStatus,
        string outputJson,
        CancellationToken cancellationToken)
    {
        var now = _clock.Now;

        if (string.Equals(taskStatus, "failed", StringComparison.OrdinalIgnoreCase))
        {
            job.Status = ProductionJobStatus.Failed;
            job.CurrentStage = ProductionStage.FailedRetryable;
            job.ErrorMessage = ExtractEnvelopeError(outputJson) ?? $"{stage.Label} 执行失败。";
            await _jobs.UpdateAsync(job, cancellationToken);
            return;
        }

        if (string.Equals(taskStatus, "review", StringComparison.OrdinalIgnoreCase))
        {
            job.Status = ProductionJobStatus.Paused;
            job.CurrentStage = stage.Stage;
            job.ProgressPercent = ProductionStageCatalog.ProgressFor(stage.Stage);
            job.ErrorMessage = null;
            await _jobs.UpdateAsync(job, cancellationToken);
            return;
        }

        if (IsLastStage(stage))
        {
            job.Status = ProductionJobStatus.Completed;
            job.CurrentStage = ProductionStage.Completed;
            job.ProgressPercent = 100;
            job.FinishedAt = now;
            job.ErrorMessage = null;
            await _jobs.UpdateAsync(job, cancellationToken);

            var project = await _projects.GetAsync(job.ProjectId, cancellationToken);
            if (project is not null)
            {
                project.Status = ProjectStatus.Completed;
                project.UpdatedAt = now;
                await _projects.UpdateAsync(project, cancellationToken);
            }

            return;
        }

        job.Status = ProductionJobStatus.Running;
        job.CurrentStage = stage.Stage;
        job.ProgressPercent = ProductionStageCatalog.ProgressFor(stage.Stage);
        job.ErrorMessage = null;
        await _jobs.UpdateAsync(job, cancellationToken);
    }

    private async Task<ProductionSkillExecutionResult> PersistFailureAsync(
        GenerationTask task,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        var errorJson = JsonSerializer.Serialize(
            new
            {
                ok = false,
                skill_name = task.SkillName,
                schema_version = "1.0",
                data = (object?)null,
                error = new
                {
                    code,
                    message,
                    details = Array.Empty<string>()
                },
                runtime = new
                {
                    duration_ms = 0,
                    model = "none",
                    mode = "worker-adapter",
                    cost_estimate = 0
                }
            },
            JsonOptions);

        await _persistence.PersistAsync(
            task.Id,
            new PersistSkillEnvelopeRequest(errorJson, "skill_error_envelope", "worker", "none", "skill_envelope", 1, 0, 0, false),
            cancellationToken);

        return new ProductionSkillExecutionResult(task.Id, task.SkillName, "failed", message);
    }

    private static bool IsLastStage(ProductionStageDefinition stage)
    {
        return ProductionStageCatalog.IndexOf(stage.Stage) == ProductionStageCatalog.All.Count - 1;
    }

    private static decimal? ExtractRuntimeCost(string outputJson)
    {
        try
        {
            using var document = JsonDocument.Parse(outputJson);
            if (document.RootElement.TryGetProperty("runtime", out var runtime) &&
                runtime.TryGetProperty("cost_estimate", out var value) &&
                value.TryGetDecimal(out var cost))
            {
                return cost;
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static string? ExtractEnvelopeError(string outputJson)
    {
        try
        {
            using var document = JsonDocument.Parse(outputJson);
            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.Object &&
                error.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static string BuildResultMessage(string skillName, string status)
    {
        return status switch
        {
            "review" => $"{skillName} 已生成，等待 checkpoint 确认。",
            "failed" => $"{skillName} 执行失败。",
            _ => $"{skillName} 已完成。"
        };
    }
}

public sealed record ProductionSkillExecutionResult(
    string TaskId,
    string SkillName,
    string Status,
    string Message);
