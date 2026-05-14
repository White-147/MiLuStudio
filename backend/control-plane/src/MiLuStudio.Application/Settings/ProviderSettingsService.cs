namespace MiLuStudio.Application.Settings;

using MiLuStudio.Application.Abstractions;

public sealed class ProviderSettingsService
{
    private const string ProviderConfigMode = "stage23_openai_compatible_config";
    private const string Stage23SafetyStage = "stage23_provider_connection_preflight";
    private const string SandboxMode = "stage23_connection_test_only";
    private const string SpendGuardMode = "connection_test_allowed_generation_blocked";
    private static readonly TimeSpan ConnectivityTimeout = TimeSpan.FromSeconds(12);

    private readonly IClock _clock;
    private readonly IProviderConnectivityTester _connectivityTester;
    private readonly IProviderSecretStore _secrets;
    private readonly IProviderSettingsRepository _settings;

    public ProviderSettingsService(
        IClock clock,
        IProviderSettingsRepository settings,
        IProviderSecretStore secrets,
        IProviderConnectivityTester connectivityTester)
    {
        _clock = clock;
        _settings = settings;
        _secrets = secrets;
        _connectivityTester = connectivityTester;
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

    public async Task<ProviderConnectionTestResponse> TestConnectionAsync(
        string kind,
        ProviderConnectionTestRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedKind = NormalizeKind(kind);
        var item = ProviderAdapterCatalog.Get(normalizedKind);
        var state = await LoadStateAsync(cancellationToken);
        var adapter = state.Adapters.First(entry =>
            string.Equals(entry.Kind, normalizedKind, StringComparison.OrdinalIgnoreCase));
        var supplier = NormalizeSupplier(item, request.Supplier ?? adapter.Supplier);
        if (string.Equals(supplier, ProviderAdapterCatalog.NoneSupplier, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProviderSettingsValidationException("Select a provider supplier before testing connectivity.");
        }

        var model = NormalizeText(request.Model ?? adapter.Model);
        var baseUrl = NormalizeBaseUrl(request.BaseUrl ?? adapter.BaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ProviderSettingsValidationException("Base URL is required before testing connectivity.");
        }

        var apiKey = NormalizeText(request.ApiKey ?? string.Empty);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = NormalizeText(await _secrets.GetSecretAsync(normalizedKind, cancellationToken) ?? string.Empty);
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new ProviderConnectionTestResponse(
                Ok: false,
                "missing_api_key",
                "API key is not configured. Enter a key or save one before testing connectivity.",
                normalizedKind,
                supplier,
                model,
                baseUrl,
                HttpStatusCode: null,
                DurationMs: 0,
                Array.Empty<string>(),
                new Dictionary<string, string>
                {
                    ["secretStore"] = "no_provider_call_secret_available"
                });
        }

        return await _connectivityTester.TestAsync(
            new ProviderConnectionTestContext(
                normalizedKind,
                supplier,
                model,
                baseUrl,
                apiKey,
                ConnectivityTimeout),
            cancellationToken);
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
            "stage23_generation_provider_calls_blocked",
            "connection_tests_allowed",
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
            "budget_passed_generation_provider_blocked",
            $"{normalizedKind} is within the configured spend guard, but Stage 23 only allows provider connection tests.",
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
                BaseUrl = NormalizeBaseUrl(adapter.BaseUrl),
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
            ProviderConfigMode,
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
            state.BaseUrl,
            state.Enabled,
            state.ApiKeyConfigured,
            state.ApiKeyPreview,
            state.SecretFingerprint,
            catalogItem.SupportedSuppliers,
            catalogItem.CapabilityFlags,
            new ProviderAdapterSafetyDto(
                CreateSecretReferenceId(state.Kind, state.SecretFingerprint),
                safety.SecretStore.Mode,
                RawSecretPersisted: safety.SecretStore.RawSecretPersistenceAllowed,
                UsableForProviderCalls: safety.SecretStore.ProviderCallSecretsAvailable,
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
                "Stage 23 stores provider keys with local Windows user encryption so connection tests can use them without exposing raw key material.",
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
                "Stage 23 allows provider connection tests only. Real generation calls, media reads, FFmpeg, and real artifact generation remain separately gated.",
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
                ["baseUrl"] = adapter.BaseUrl,
                ["adapterMode"] = ProviderConfigMode,
                ["secretStore"] = safety.SecretStore.Mode,
                ["sandbox"] = safety.Sandbox.Mode,
                ["externalNetwork"] = "connection_test_only",
                ["providerCalls"] = "generation_blocked",
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

            if (string.IsNullOrWhiteSpace(adapter.BaseUrl))
            {
                missing.Add("baseUrl");
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
                "Local provider configuration is complete for connection testing. Real generation calls remain disabled.",
                details);
        }));

        var healthy = checks.All(check => !string.Equals(check.Status, "warning", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(check.Status, "error", StringComparison.OrdinalIgnoreCase));
        var recommendations = new List<string>
        {
            "Stage 23 allows lightweight OpenAI-compatible connection tests only.",
            "No story, image, video, audio, subtitle, package, or final artifact generation is sent to a real provider.",
            "Future real provider generation must pass through the secret store, spend guard, sandbox checks, and audit logging first."
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
            ExternalNetworkAllowed: true,
            MediaReadAllowed: false,
            FfmpegAllowed: false,
            ProviderAdapterCatalog.Items.Select(item => item.Kind).ToArray(),
            ["connection_test_json_only", "no_generation_payloads", "no_real_png", "no_real_mp4", "no_real_wav", "no_real_srt", "no_real_zip"]);
        var spendGuard = new ProviderSpendGuardStatusDto(
            Enabled: true,
            SpendGuardMode,
            state.CostGuardrails.ProjectCostCapCny,
            state.CostGuardrails.RetryLimit,
            BlocksProviderCalls: false,
            BlocksWhenCapExceeded: true);

        return new ProviderSafetyStatusDto(
            Stage23SafetyStage,
            ProviderConfigMode,
            secretStore,
            spendGuard,
            sandbox,
            [
                "real_generation_provider_calls_disabled",
                "external_network_limited_to_connection_test",
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
            item.DefaultBaseUrl,
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
                Model = NormalizeText(baseline.Model),
                BaseUrl = NormalizeBaseUrl(baseline.BaseUrl)
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
            NormalizeBaseUrl(requested.BaseUrl),
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

    private static string NormalizeText(string? value) => (value ?? string.Empty).Trim();

    private static string NormalizeBaseUrl(string? value)
    {
        var normalized = NormalizeText(value).TrimEnd('/');
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        if (normalized.Length > 512)
        {
            throw new ProviderSettingsValidationException("Base URL must be 512 characters or fewer.");
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ProviderSettingsValidationException("Base URL must be an absolute http(s) URL without query or fragment.");
        }

        return normalized;
    }

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
    string DefaultBaseUrl,
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
            string.Empty,
            [NoneSupplier, "openai_compatible", "openai", "qwen", "volcano", "anthropic"],
            ["story", "script", "storyboard", "rewrite"]),
        new(
            "image",
            "图像生成",
            "image-placeholder-v1",
            string.Empty,
            [NoneSupplier, "openai_compatible", "openai", "qwen", "volcano", "stability"],
            ["character", "keyframe", "style-reference"]),
        new(
            "video",
            "视频生成",
            "video-placeholder-v1",
            string.Empty,
            [NoneSupplier, "openai_compatible", "runway", "pika", "volcano", "kling"],
            ["shot-render", "motion", "upscale"]),
        new(
            "audio",
            "音频生成",
            "audio-placeholder-v1",
            string.Empty,
            [NoneSupplier, "openai_compatible", "elevenlabs", "minimax", "azure", "volcano"],
            ["voice", "music", "sound-effect"]),
        new(
            "edit",
            "编辑与质检",
            "edit-placeholder-v1",
            string.Empty,
            [NoneSupplier, "openai_compatible", "openai", "qwen", "volcano"],
            ["qc", "rewrite", "caption", "repair"])
    ];

    public static ProviderAdapterCatalogItem Get(string kind) =>
        Items.First(item => string.Equals(item.Kind, kind, StringComparison.OrdinalIgnoreCase));
}
