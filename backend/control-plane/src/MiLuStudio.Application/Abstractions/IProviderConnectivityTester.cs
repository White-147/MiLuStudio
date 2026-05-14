namespace MiLuStudio.Application.Abstractions;

using MiLuStudio.Application.Settings;

public interface IProviderConnectivityTester
{
    Task<ProviderConnectionTestResponse> TestAsync(
        ProviderConnectionTestContext context,
        CancellationToken cancellationToken);
}

public sealed record ProviderConnectionTestContext(
    string Kind,
    string Supplier,
    string Model,
    string BaseUrl,
    string ApiKey,
    TimeSpan Timeout);
