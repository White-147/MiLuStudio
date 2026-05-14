namespace MiLuStudio.Infrastructure.Settings;

using Stopwatch = global::System.Diagnostics.Stopwatch;
using AuthenticationHeaderValue = global::System.Net.Http.Headers.AuthenticationHeaderValue;
using MediaTypeWithQualityHeaderValue = global::System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
using JsonDocument = global::System.Text.Json.JsonDocument;
using JsonElement = global::System.Text.Json.JsonElement;
using JsonException = global::System.Text.Json.JsonException;
using JsonValueKind = global::System.Text.Json.JsonValueKind;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Settings;

public sealed class OpenAiCompatibleProviderConnectivityTester : IProviderConnectivityTester
{
    public async Task<ProviderConnectionTestResponse> TestAsync(
        ProviderConnectionTestContext context,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoints = CandidateModelEndpoints(context.BaseUrl);
        var details = new Dictionary<string, string>
        {
            ["testMode"] = "openai_compatible_models_endpoint",
            ["generationPayloadSent"] = "false"
        };

        using var http = new HttpClient
        {
            Timeout = context.Timeout
        };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.ApiKey);
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        ProviderConnectionTestResponse? lastResponse = null;
        foreach (var endpoint in endpoints)
        {
            try
            {
                using var response = await http.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                var bodyPreview = await ReadPreviewAsync(response, cancellationToken);
                stopwatch.Stop();

                details["finalEndpoint"] = endpoint;
                if (response.IsSuccessStatusCode)
                {
                    var modelCount = TryReadModelCount(bodyPreview);
                    if (modelCount is not null)
                    {
                        details["modelCount"] = modelCount.Value.ToString();
                    }

                    return new ProviderConnectionTestResponse(
                        Ok: true,
                        "connected",
                        "Provider connection test succeeded. No generation request was sent.",
                        context.Kind,
                        context.Supplier,
                        context.Model,
                        context.BaseUrl,
                        (int)response.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        endpoints,
                        details);
                }

                details["lastBodyPreview"] = bodyPreview;
                lastResponse = new ProviderConnectionTestResponse(
                    Ok: false,
                    $"http_{(int)response.StatusCode}",
                    $"Provider returned HTTP {(int)response.StatusCode}. Check Base URL, API key, and relay compatibility.",
                    context.Kind,
                    context.Supplier,
                    context.Model,
                    context.BaseUrl,
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    endpoints,
                    new Dictionary<string, string>(details));

                if ((int)response.StatusCode != 404)
                {
                    return lastResponse;
                }
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                return new ProviderConnectionTestResponse(
                    Ok: false,
                    "timeout",
                    $"Provider connection test timed out after {context.Timeout.TotalSeconds:F0} seconds.",
                    context.Kind,
                    context.Supplier,
                    context.Model,
                    context.BaseUrl,
                    HttpStatusCode: null,
                    stopwatch.ElapsedMilliseconds,
                    endpoints,
                    details);
            }
            catch (HttpRequestException error)
            {
                stopwatch.Stop();
                details["networkError"] = error.Message;
                return new ProviderConnectionTestResponse(
                    Ok: false,
                    "network_error",
                    "Provider connection test failed before receiving an HTTP response.",
                    context.Kind,
                    context.Supplier,
                    context.Model,
                    context.BaseUrl,
                    HttpStatusCode: null,
                    stopwatch.ElapsedMilliseconds,
                    endpoints,
                    details);
            }
        }

        stopwatch.Stop();
        return lastResponse ?? new ProviderConnectionTestResponse(
            Ok: false,
            "not_tested",
            "Provider connection test did not run.",
            context.Kind,
            context.Supplier,
            context.Model,
            context.BaseUrl,
            HttpStatusCode: null,
            stopwatch.ElapsedMilliseconds,
            endpoints,
            details);
    }

    private static IReadOnlyList<string> CandidateModelEndpoints(string baseUrl)
    {
        var normalized = baseUrl.TrimEnd('/');
        var endpoints = new List<string> { $"{normalized}/models" };
        if (!normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            endpoints.Add($"{normalized}/v1/models");
        }

        return endpoints.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static async Task<string> ReadPreviewAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        return text.Length > 800 ? text[..800] : text;
    }

    private static int? TryReadModelCount(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array
                ? data.GetArrayLength()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
