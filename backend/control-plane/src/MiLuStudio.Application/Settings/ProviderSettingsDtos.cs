namespace MiLuStudio.Application.Settings;

public sealed record ProviderSettingsResponse(
    string Mode,
    DateTimeOffset UpdatedAt,
    ProviderCostGuardrailsDto CostGuardrails,
    IReadOnlyList<ProviderAdapterSettingsDto> Adapters,
    ProviderSettingsPreflightDto Preflight,
    ProviderSafetyStatusDto Safety);

public sealed record ProviderAdapterSettingsDto(
    string Kind,
    string Label,
    string Supplier,
    string Model,
    string BaseUrl,
    bool Enabled,
    bool ApiKeyConfigured,
    string ApiKeyPreview,
    string SecretFingerprint,
    IReadOnlyList<string> SupportedSuppliers,
    IReadOnlyList<string> CapabilityFlags,
    ProviderAdapterSafetyDto Safety);

public sealed record ProviderCostGuardrailsDto(
    decimal ProjectCostCapCny,
    int RetryLimit);

public sealed record ProviderSettingsPreflightDto(
    bool Healthy,
    IReadOnlyList<ProviderPreflightCheckDto> Checks,
    IReadOnlyList<string> Recommendations);

public sealed record ProviderPreflightCheckDto(
    string Kind,
    string Label,
    string Status,
    string Message,
    IReadOnlyDictionary<string, string> Details);

public sealed record ProviderSafetyStatusDto(
    string Stage,
    string Mode,
    ProviderSecretStoreStatusDto SecretStore,
    ProviderSpendGuardStatusDto SpendGuard,
    ProviderSandboxStatusDto Sandbox,
    IReadOnlyList<string> BlockingReasons);

public sealed record ProviderAdapterSafetyDto(
    string SecretReferenceId,
    string SecretStoreMode,
    bool RawSecretPersisted,
    bool UsableForProviderCalls,
    string SandboxMode,
    bool ProviderCallsAllowed,
    bool ExternalNetworkAllowed,
    bool MediaReadAllowed,
    bool FfmpegAllowed);

public sealed record ProviderSecretStoreStatusDto(
    string Mode,
    string StorageScope,
    bool MetadataStoreAvailable,
    bool RawSecretPersistenceAllowed,
    bool ProviderCallSecretsAvailable,
    IReadOnlyList<string> Checks);

public sealed record ProviderSpendGuardStatusDto(
    bool Enabled,
    string EnforcementMode,
    decimal ProjectCostCapCny,
    int RetryLimit,
    bool BlocksProviderCalls,
    bool BlocksWhenCapExceeded);

public sealed record ProviderSandboxStatusDto(
    string Mode,
    bool ProviderCallsAllowed,
    bool ExternalNetworkAllowed,
    bool MediaReadAllowed,
    bool FfmpegAllowed,
    IReadOnlyList<string> AllowedAdapterKinds,
    IReadOnlyList<string> OutputContract);

public sealed record ProviderSpendGuardCheckRequest(
    string ProjectId,
    string ProviderKind,
    decimal CurrentSpendCny,
    decimal EstimatedIncrementCny,
    int AttemptNumber);

public sealed record ProviderSpendGuardDecisionDto(
    bool BudgetAllowed,
    bool ProviderCallAllowed,
    string Decision,
    string Reason,
    decimal ProjectCostCapCny,
    decimal CurrentSpendCny,
    decimal EstimatedIncrementCny,
    decimal ProjectedSpendCny,
    int RetryLimit,
    int AttemptNumber,
    IReadOnlyList<string> AppliedRules);

public sealed record ProviderConnectionTestRequest(
    string? Supplier,
    string? Model,
    string? BaseUrl,
    string? ApiKey);

public sealed record ProviderConnectionTestResponse(
    bool Ok,
    string Status,
    string Message,
    string ProviderKind,
    string Supplier,
    string Model,
    string BaseUrl,
    int? HttpStatusCode,
    long DurationMs,
    IReadOnlyList<string> CheckedEndpoints,
    IReadOnlyDictionary<string, string> Details);

public sealed record ProviderSettingsUpdateRequest(
    ProviderCostGuardrailsDto CostGuardrails,
    IReadOnlyList<ProviderAdapterUpdateRequest> Adapters);

public sealed record ProviderAdapterUpdateRequest(
    string Kind,
    string Supplier,
    string Model,
    string? BaseUrl,
    bool Enabled,
    string? ApiKey,
    bool ClearApiKey);

public sealed record ProviderSettingsState(
    DateTimeOffset UpdatedAt,
    ProviderCostGuardrailsState CostGuardrails,
    IReadOnlyList<ProviderAdapterState> Adapters);

public sealed record ProviderCostGuardrailsState(
    decimal ProjectCostCapCny,
    int RetryLimit);

public sealed record ProviderAdapterState(
    string Kind,
    string Supplier,
    string Model,
    string BaseUrl,
    bool Enabled,
    bool ApiKeyConfigured,
    string ApiKeyPreview,
    string SecretFingerprint);

public sealed record ProviderSecretStoreState(
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ProviderSecretDescriptorState> Secrets);

public sealed record ProviderSecretDescriptorState(
    string Kind,
    string SecretReferenceId,
    string ApiKeyPreview,
    string SecretFingerprint,
    DateTimeOffset UpdatedAt,
    bool RawSecretPersisted,
    bool UsableForProviderCalls,
    string StoreMode);

public sealed class ProviderSettingsValidationException : Exception
{
    public ProviderSettingsValidationException(string message)
        : base(message)
    {
    }
}
