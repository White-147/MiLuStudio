namespace MiLuStudio.Application.Production;

using JsonDocument = global::System.Text.Json.JsonDocument;
using JsonSerializer = global::System.Text.Json.JsonSerializer;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Domain;
using MiLuStudio.Domain.Entities;

public sealed class SkillEnvelopePersistenceService
{
    private readonly IAssetRepository _assets;
    private readonly IClock _clock;
    private readonly ICostLedgerRepository _costLedger;
    private readonly IProductionJobRepository _jobs;

    public SkillEnvelopePersistenceService(
        IClock clock,
        IProductionJobRepository jobs,
        IAssetRepository assets,
        ICostLedgerRepository costLedger)
    {
        _clock = clock;
        _jobs = jobs;
        _assets = assets;
        _costLedger = costLedger;
    }

    public async Task<PersistSkillEnvelopeResponse?> PersistAsync(
        string taskId,
        PersistSkillEnvelopeRequest request,
        CancellationToken cancellationToken)
    {
        var task = await _jobs.GetTaskAsync(taskId, cancellationToken);

        if (task is null)
        {
            return null;
        }

        var envelope = ParseEnvelope(request.OutputJson);

        var now = _clock.Now;
        var requiresReview = request.RequiresReview ?? false;
        task.OutputJson = request.OutputJson;
        task.Status = envelope.Ok
            ? requiresReview ? GenerationTaskStatus.Review : GenerationTaskStatus.Completed
            : GenerationTaskStatus.Failed;
        task.FinishedAt = task.Status == GenerationTaskStatus.Review ? null : now;
        task.ErrorMessage = envelope.Ok ? null : envelope.ErrorMessage ?? "Skill envelope reported failure.";
        task.LockedUntil = null;
        task.LastHeartbeatAt = now;

        await _jobs.UpdateTaskAsync(task, cancellationToken);

        var asset = new Asset
        {
            Id = $"asset_{Guid.NewGuid():N}",
            ProjectId = task.ProjectId,
            Kind = string.IsNullOrWhiteSpace(request.AssetKind) ? "skill_envelope" : request.AssetKind.Trim(),
            LocalPath = $"db://generation_tasks/{task.Id}/output_json",
            MimeType = "application/json",
            FileSize = request.OutputJson.Length,
            Sha256 = null,
            MetadataJson = JsonSerializer.Serialize(new
            {
                taskId = task.Id,
                task.JobId,
                task.SkillName,
                persistedBy = "control_api_or_worker",
                writesFiles = false
            }),
            CreatedAt = now
        };

        await _assets.AddAsync(asset, cancellationToken);

        string? costLedgerEntryId = null;
        if (request.EstimatedCost.HasValue || request.ActualCost.HasValue)
        {
            var entry = new CostLedgerEntry
            {
                Id = $"cost_{Guid.NewGuid():N}",
                ProjectId = task.ProjectId,
                TaskId = task.Id,
                Provider = string.IsNullOrWhiteSpace(request.Provider) ? task.Provider : request.Provider.Trim(),
                Model = string.IsNullOrWhiteSpace(request.Model) ? "none" : request.Model.Trim(),
                Unit = string.IsNullOrWhiteSpace(request.Unit) ? "skill_envelope" : request.Unit.Trim(),
                Quantity = request.Quantity ?? 1,
                EstimatedCost = request.EstimatedCost ?? task.CostEstimate,
                ActualCost = request.ActualCost,
                CreatedAt = now
            };

            await _costLedger.AddAsync(entry, cancellationToken);
            costLedgerEntryId = entry.Id;
        }

        return new PersistSkillEnvelopeResponse(
            task.Id,
            task.ProjectId,
            task.JobId,
            task.SkillName,
            task.Status switch
            {
                GenerationTaskStatus.Review => "review",
                GenerationTaskStatus.Failed => "failed",
                _ => "completed"
            },
            asset.Id,
            costLedgerEntryId);
    }

    private static SkillEnvelopeState ParseEnvelope(string value)
    {
        using var document = JsonDocument.Parse(value);
        var root = document.RootElement;
        var ok = true;
        string? errorMessage = null;

        if (root.ValueKind == global::System.Text.Json.JsonValueKind.Object &&
            root.TryGetProperty("ok", out var okElement) &&
            okElement.ValueKind is global::System.Text.Json.JsonValueKind.True or global::System.Text.Json.JsonValueKind.False)
        {
            ok = okElement.GetBoolean();
        }

        if (!ok &&
            root.TryGetProperty("error", out var errorElement) &&
            errorElement.ValueKind == global::System.Text.Json.JsonValueKind.Object &&
            errorElement.TryGetProperty("message", out var messageElement))
        {
            errorMessage = messageElement.GetString();
        }

        return new SkillEnvelopeState(ok, errorMessage);
    }

    private sealed record SkillEnvelopeState(bool Ok, string? ErrorMessage);
}
