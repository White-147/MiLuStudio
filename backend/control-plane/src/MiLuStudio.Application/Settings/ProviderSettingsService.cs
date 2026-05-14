namespace MiLuStudio.Application.Settings;

using MiLuStudio.Application.Abstractions;

public sealed class ProviderSettingsService
{
    private const string PlaceholderMode = "placeholder_only";
    private const string Stage22SafetyStage = "stage22_provider_safety_preflight";
    private const string SandboxMode = "stage22_no_provider_calls";
    private const string SpendGuardMode = "hard_preflight_placeholder_block";

    private readonly IClock _clock;
    private readonly IProviderSecretStore _secrets;
    private readonly IProviderSettingsRepository _settings;

    public ProviderSettingsService(
        IClock clock,
        IProviderSettingsRepository settings,
        IProviderSecretStore secrets)
    {
        _clock = clock;
        _settings = settings;
        _secrets = secrets;
    }

    public async Task<ProviderSettingsResponse> GetAsync(CancellationToken cancellationToken)
    {
        var state = await LoadStateAsync(cancellationToken);
        return await ToResponseAsync(state, cancellationToken);
    }

    public async Task<ProviderSettingsResponse> UpdateAsync(
        ProviderSettingsUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var current = await LoadStateAsync(cancellationToken);
        var currentByKind = current.Adapters.ToDictionary(adapter => adapter.Kind, StringComparer.OrdinalIgnoreCase);
        var requestedByKind = request.Adapters.ToDictionary(adapter => NormalizeKind(adapter.Kind), StringComparer.OrdinalIgnoreCase);

        var now = _clock.Now;
        var costGuardrails = NormalizeCostGuardrails(request.CostGuardrails);
        var adapters = new List<ProviderAdapterState>();
        foreach (var item in ProviderAdapterCatalog.Items)
        {
            requestedByKind.TryGetValue(item.Kind, out var requested);
            currentByKind.TryGetValue(item.Kind, out var existing);
            adapters.Add(await ApplyAdapterUpdateAsync(item, existing, requested, now, cancellationToken));
        }

        var nextState = new ProviderSettingsState(now, costGuardrails, adapters);
        await _settings.SaveAsync(nextState, cancellationToken);
        return await ToResponseAsync(nextState, cancellationToken);
    }

    public async Task<ProviderSettingsPreflightDto> CheckAsync(CancellationToken cancellationToken)
    {
        var state = await LoadStateAsync(cancellationToken);
        var safety = await BuildSafetyStatusAsync(state, cancellationToken);
        return BuildPreflight(state, safety);
    }

    public async Task<ProviderSafetyStatusDto> GetSafetyAsync(CancellationToken cancellationToken)
    {
        var state = await LoadStateAsync(cancellationToken);
        return await BuildSafetyStatusAsync(state, cancellationToken);
    }

    public async Task<ProviderSpendGuardDecisionDto> CheckSpendGuardAsync(
        ProviderSpendGuardCheckRequest request,
        CancellationToken cancellationToken)
    {
        var state = await LoadStateAsync(cancellationToken);
        var normalizedKind = NormalizeKind(request.ProviderKind);
        var currentSpend = NormalizeMoney(request.CurrentSpendCny, nameof(request.CurrentSpendCny));
        var increment = NormalizeMoney(request.EstimatedIncrementCny, nameof(request.EstimatedIncrementCny));
        var projected = decimal.Round(currentSpend + increment, 2);
        var retryLimit = state.CostGuardrails.RetryLimit;
        var attemptNumber = Math.Max(1, request.AttemptNumber);
        var appliedRules = new List<string>
        {
            "stage22_provider_calls_blocked",
            "project_cost_cap_enforced",
            "retry_limit_enforced"
        };

        if (projected > state.CostGuardrails.ProjectCostCapCny)
        {
            return new ProviderSpendGuardDecisionDto(
                BudgetAllowed: false,
                ProviderCallAllowed: false,
                "budget_blocked",
                $"Projected {projected:F2} CNY exceeds project cap {state.CostGuardrails.ProjectCostCapCny:F2} CNY.",
                state.CostGuardrails.ProjectCostCapCny,
                currentSpend,
                increment,
                projected,
                retryLimit,
                attemptNumber,
                appliedRules);
        }

        if (attemptNumber > retryLimit + 1)
        {
            return new ProviderSpendGuardDecisionDto(
                BudgetAllowed: false,
                ProviderCallAllowed: false,
                "retry_blocked",
                $"Attempt {attemptNumber} exceeds retry limit {retryLimit}.",
                state.CostGuardrails.ProjectCostCapCny,
                currentSpend,
                increment,
                projected,
                retryLimit,
                attemptNumber,
                appliedRules);
        }

        return new ProviderSpendGuardDecisionDto(
            BudgetAllowed: true,
            ProviderCallAllowed: false,
            "budget_passed_provider_blocked",
            $"{normalizedKind} is within the configured spend guard, but Stage 22 keeps real provider calls sandbox-blocked.",
            state.CostGuardrails.ProjectCostCapCny,
            currentSpend,
            increment,
            projected,
            retryLimit,
            attemptNumber,
            appliedRules);
    }

    private async Task<ProviderSettingsState> LoadStateAsync(CancellationToken cancellationToken)
    {
        var stored = await _settings.GetAsync(cancellationToken);
        if (stored is null)
        {
            return CreateDefaultState(_clock.Now);
        }

        var storedByKind = stored.Adapters.ToDictionary(adapter => adapter.Kind, StringComparer.OrdinalIgnoreCase);
        var adapters = ProviderAdapterCatalog.Items.Select(item =>
        {
            if (!storedByKind.TryGetValue(item.Kind, out var adapter))
            {
                return CreateDefaultAdapter(item);
            }

            return adapter with
            {
                Kind = item.Kind,
                Supplier = NormalizeSupplier(item, adapter.Supplier),
                Model = NormalizeText(adapter.Model),
                ApiKeyPreview = NormalizeText(adapter.ApiKeyPreview),
                SecretFingerprint = NormalizeText(adapter.SecretFingerprint)
            };
        }).ToArray();

        return stored with
        {
            CostGuardrails = NormalizeCostGuardrails(new ProviderCostGuardrailsDto(
                stored.CostGuardrails.ProjectCostCapCny,
                stored.CostGuardrails.RetryLimit)),
            Adapters = adapters
        };
    }

    private async Task<ProviderSettingsResponse> ToResponseAsync(
        ProviderSettingsState state,
        CancellationToken cancellationToken)
    {
        var safety = await BuildSafetyStatusAsync(state, cancellationToken);
        return new ProviderSettingsResponse(
            PlaceholderMode,
            state.UpdatedAt,
            new ProviderCostGuardrailsDto(
                state.CostGuardrails.ProjectCostCapCny,
                state.CostGuardrails.RetryLimit),
            state.Adapters.Select(adapter => ToAdapterDto(adapter, safety)).ToArray(),
            BuildPreflight(state, safety),
            safety);
    }

    private static ProviderAdapterSettingsDto ToAdapterDto(
        ProviderAdapterState state,
        ProviderSafetyStatusDto safety)
    {
        var catalogItem = ProviderAdapterCatalog.Get(state.Kind);
        return new ProviderAdapterSettingsDto(
            catalogItem.Kind,
            catalogItem.Label,
            state.Supplier,
            state.Model,
            state.Enabled,
            state.ApiKeyConfigured,
            state.ApiKeyPreview,
            state.SecretFingerprint,
            catalogItem.SupportedSuppliers,
            catalogItem.CapabilityFlags,
            new ProviderAdapterSafetyDto(
                CreateSecretReferenceId(state.Kind, state.SecretFingerprint),
                safety.SecretStore.Mode,
                RawSecretPersisted: false,
                UsableForProviderCalls: false,
                safety.Sandbox.Mode,
                safety.Sandbox.ProviderCallsAllowed,
                safety.Sandbox.ExternalNetworkAllowed,
                safety.Sandbox.MediaReadAllowed,
                safety.Sandbox.FfmpegAllowed));
    }

    private static ProviderSettingsPreflightDto BuildPreflight(
        ProviderSettingsState state,
        ProviderSafetyStatusDto safety)
    {
        var checks = new List<ProviderPreflightCheckDto>
        {
            new(
                "secret_store",
                "Secure Secret Store",
                safety.SecretStore.MetadataStoreAvailable ? "ok" : "error",
                "Stage 22 stores provider secret metadata only; raw key material is not persisted or available for calls.",
                new Dictionary<string, string>
                {
                    ["mode"] = safety.SecretStore.Mode,
                    ["rawSecretPersistenceAllowed"] = safety.SecretStore.RawSecretPersistenceAllowed.ToString().ToLowerInvariant(),
                    ["providerCallSecretsAvailable"] = safety.SecretStore.ProviderCallSecretsAvailable.ToString().ToLowerInvariant()
                }),
            new(
                "spend_guard",
                "Spend Guard",
                safety.SpendGuard.Enabled ? "ok" : "error",
                "Project cost cap and retry limit are enforced before any future provider execution.",
                new Dictionary<string, string>
                {
                    ["enforcementMode"] = safety.SpendGuard.EnforcementMode,
                    ["projectCostCapCny"] = safety.SpendGuard.ProjectCostCapCny.ToString("F2"),
                    ["retryLimit"] = safety.SpendGuard.RetryLimit.ToString()
                }),
            new(
                "provider_sandbox",
                "Provider Sandbox",
                "ok",
                "Stage 22 sandbox blocks provider calls, external network, media reads, FFmpeg, and real artifact generation.",
                new Dictionary<string, string>
                {
                    ["mode"] = safety.Sandbox.Mode,
                    ["providerCallsAllowed"] = safety.Sandbox.ProviderCallsAllowed.ToString().ToLowerInvariant(),
                    ["externalNetworkAllowed"] = safety.Sandbox.ExternalNetworkAllowed.ToString().ToLowerInvariant(),
                    ["mediaReadAllowed"] = safety.Sandbox.MediaReadAllowed.ToString().ToLowerInvariant(),
                    ["ffmpegAllowed"] = safety.Sandbox.FfmpegAllowed.ToString().ToLowerInvariant()
                })
        };

        checks.AddRange(state.Adapters.Select(adapter =>
        {
            var catalogItem = ProviderAdapterCatalog.Get(adapter.Kind);
            var details = new Dictionary<string, string>
            {
                ["supplier"] = adapter.Supplier,
                ["model"] = adapter.Model,
                ["adapterMode"] = PlaceholderMode,
                ["secretStore"] = safety.SecretStore.Mode,
                ["sandbox"] = safety.Sandbox.Mode,
                ["externalNetwork"] = "disabled",
                ["providerCalls"] = "blocked",
                ["mediaGenerated"] = "false",
                ["mediaRead"] = "false",
                ["ffmpegInvoked"] = "false"
            };

            if (!adapter.Enabled)
            {
                return new ProviderPreflightCheckDto(
                    adapter.Kind,
                    catalogItem.Label,
                    "skipped",
                    "Adapter is disabled. Deterministic local pipeline remains active.",
                    details);
            }

            var missing = new List<string>();
            if (string.Equals(adapter.Supplier, ProviderAdapterCatalog.NoneSupplier, StringComparison.OrdinalIgnoreCase))
            {
                missing.Add("supplier");
            }

            if (string.IsNullOrWhiteSpace(adapter.Model))
            {
                missing.Add("model");
            }

            if (!adapter.ApiKeyConfigured)
            {
                missing.Add("apiKey");
            }

            if (missing.Count > 0)
            {
                details["missing"] = string.Join(",", missing);
                return new ProviderPreflightCheckDto(
                    adapter.Kind,
                    catalogItem.Label,
                    "warning",
                    "Adapter is enabled but its local placeholder configuration is incomplete.",
                    details);
            }

            details["secretReferenceId"] = CreateSecretReferenceId(adapter.Kind, adapter.SecretFingerprint);
            return new ProviderPreflightCheckDto(
                adapter.Kind,
                catalogItem.Label,
                "ok",
                "Local safety metadata is complete. Real provider calls remain sandbox-blocked.",
                details);
        }));

        var healthy = checks.All(check => !string.Equals(check.Status, "warning", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(check.Status, "error", StringComparison.OrdinalIgnoreCase));
        var recommendations = new List<string>
        {
            "Stage 22 keeps all provider adapters in placeholder mode with sandbox-blocked execution.",
            "No external model request, media read, FFmpeg run, or real artifact generation is performed.",
            "Future real provider wiring must pass through the secret store, spend guard, and sandbox checks first."
        };

        if (!healthy)
        {
            recommendations.Insert(0, "Complete or disable every enabled provider before future real adapter wiring.");
        }

        return new ProviderSettingsPreflightDto(healthy, checks, recommendations);
    }

    private async Task<ProviderSafetyStatusDto> BuildSafetyStatusAsync(
        ProviderSettingsState state,
        CancellationToken cancellationToken)
    {
        var secretStore = await _secrets.GetStatusAsync(cancellationToken);
        var sandbox = new ProviderSandboxStatusDto(
            SandboxMode,
            ProviderCallsAllowed: false,
            ExternalNetworkAllowed: false,
            MediaReadAllowed: false,
            FfmpegAllowed: false,
            ProviderAdapterCatalog.Items.Select(item => item.Kind).ToArray(),
            ["json_envelope_only", "no_real_png", "no_real_mp4", "no_real_wav", "no_real_srt", "no_real_zip"]);
        var spendGuard = new ProviderSpendGuardStatusDto(
            Enabled: true,
            SpendGuardMode,
            state.CostGuardrails.ProjectCostCapCny,
            state.CostGuardrails.RetryLimit,
            BlocksProviderCalls: true,
            BlocksWhenCapExceeded: true);

        return new ProviderSafetyStatusDto(
            Stage22SafetyStage,
            PlaceholderMode,
            secretStore,
            spendGuard,
            sandbox,
            [
                "real_provider_calls_disabled",
                "external_network_disabled",
                "raw_secret_persistence_disabled",
                "media_file_reads_disabled",
                "ffmpeg_disabled",
                "real_artifact_generation_disabled"
            ]);
    }

    private static ProviderSettingsState CreateDefaultState(DateTimeOffset now) =>
        new(
            now,
            new ProviderCostGuardrailsState(50, 1),
            ProviderAdapterCatalog.Items.Select(CreateDefaultAdapter).ToArray());

    private static ProviderAdapterState CreateDefaultAdapter(ProviderAdapterCatalogItem item) =>
        new(
            item.Kind,
            ProviderAdapterCatalog.NoneSupplier,
            item.DefaultModel,
            false,
            false,
            string.Empty,
            string.Empty);

    private async Task<ProviderAdapterState> ApplyAdapterUpdateAsync(
        ProviderAdapterCatalogItem item,
        ProviderAdapterState? existing,
        ProviderAdapterUpdateRequest? requested,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var baseline = existing ?? CreateDefaultAdapter(item);
        if (requested is null)
        {
            return baseline with
            {
                Kind = item.Kind,
                Supplier = NormalizeSupplier(item, baseline.Supplier),
                Model = NormalizeText(baseline.Model)
            };
        }

        var apiKeyConfigured = baseline.ApiKeyConfigured;
        var apiKeyPreview = baseline.ApiKeyPreview;
        var secretFingerprint = baseline.SecretFingerprint;

        if (requested.ClearApiKey)
        {
            await _secrets.ClearAsync(item.Kind, now, cancellationToken);
            apiKeyConfigured = false;
            apiKeyPreview = string.Empty;
            secretFingerprint = string.Empty;
        }
        else if (!string.IsNullOrWhiteSpace(requested.ApiKey))
        {
            var descriptor = await _secrets.SaveMetadataAsync(
                item.Kind,
                requested.ApiKey.Trim(),
                now,
                cancellationToken);
            apiKeyConfigured = true;
            apiKeyPreview = descriptor.ApiKeyPreview;
            secretFingerprint = descriptor.SecretFingerprint;
        }

        return new ProviderAdapterState(
            item.Kind,
            NormalizeSupplier(item, requested.Supplier),
            NormalizeText(requested.Model),
            requested.Enabled,
            apiKeyConfigured,
            apiKeyPreview,
            secretFingerprint);
    }

    private static ProviderCostGuardrailsState NormalizeCostGuardrails(ProviderCostGuardrailsDto costGuardrails)
    {
        if (costGuardrails.ProjectCostCapCny < 0 || costGuardrails.ProjectCostCapCny > 100000)
        {
            throw new ProviderSettingsValidationException("Project cost cap must be between 0 and 100000 CNY.");
        }

        if (costGuardrails.RetryLimit < 0 || costGuardrails.RetryLimit > 5)
        {
            throw new ProviderSettingsValidationException("Retry limit must be between 0 and 5.");
        }

        return new ProviderCostGuardrailsState(
            decimal.Round(costGuardrails.ProjectCostCapCny, 2),
            costGuardrails.RetryLimit);
    }

    private static string NormalizeKind(string kind)
    {
        var normalized = NormalizeText(kind).ToLowerInvariant();
        if (ProviderAdapterCatalog.Items.Any(item => item.Kind == normalized))
        {
            return normalized;
        }

        throw new ProviderSettingsValidationException($"Unsupported provider kind '{kind}'.");
    }

    private static string NormalizeSupplier(ProviderAdapterCatalogItem item, string supplier)
    {
        var normalized = NormalizeText(supplier).ToLowerInvariant();
        if (normalized.Length == 0)
        {
            return ProviderAdapterCatalog.NoneSupplier;
        }

        if (item.SupportedSuppliers.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            return normalized;
        }

        throw new ProviderSettingsValidationException(
            $"Unsupported supplier '{supplier}' for provider kind '{item.Kind}'.");
    }

    private static decimal NormalizeMoney(decimal value, string fieldName)
    {
        if (value < 0 || value > 100000)
        {
            throw new ProviderSettingsValidationException($"{fieldName} must be between 0 and 100000 CNY.");
        }

        return decimal.Round(value, 2);
    }

    private static string NormalizeText(string value) => (value ?? string.Empty).Trim();

    private static string CreateSecretReferenceId(string kind, string fingerprint)
    {
        if (string.IsNullOrWhiteSpace(fingerprint))
        {
            return string.Empty;
        }

        return $"provider-secret:{kind.Trim().ToLowerInvariant()}";
    }
}

internal sealed record ProviderAdapterCatalogItem(
    string Kind,
    string Label,
    string DefaultModel,
    IReadOnlyList<string> SupportedSuppliers,
    IReadOnlyList<string> CapabilityFlags);

internal static class ProviderAdapterCatalog
{
    public const string NoneSupplier = "none";

    public static readonly IReadOnlyList<ProviderAdapterCatalogItem> Items =
    [
        new(
            "text",
            "文本生成",
            "text-placeholder-v1",
            [NoneSupplier, "openai", "qwen", "volcano", "anthropic"],
            ["story", "script", "storyboard", "rewrite"]),
        new(
            "image",
            "图像生成",
            "image-placeholder-v1",
            [NoneSupplier, "openai", "qwen", "volcano", "stability"],
            ["character", "keyframe", "style-reference"]),
        new(
            "video",
            "视频生成",
            "video-placeholder-v1",
            [NoneSupplier, "runway", "pika", "volcano", "kling"],
            ["shot-render", "motion", "upscale"]),
        new(
            "audio",
            "音频生成",
            "audio-placeholder-v1",
            [NoneSupplier, "elevenlabs", "minimax", "azure", "volcano"],
            ["voice", "music", "sound-effect"]),
        new(
            "edit",
            "编辑与质检",
            "edit-placeholder-v1",
            [NoneSupplier, "openai", "qwen", "volcano"],
            ["qc", "rewrite", "caption", "repair"])
    ];

    public static ProviderAdapterCatalogItem Get(string kind) =>
        Items.First(item => string.Equals(item.Kind, kind, StringComparison.OrdinalIgnoreCase));
}
