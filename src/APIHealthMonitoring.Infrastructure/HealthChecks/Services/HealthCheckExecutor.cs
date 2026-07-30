using System.Diagnostics;
using APIHealthMonitoring.Application.Interfaces.HealthChecks;
using APIHealthMonitoring.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace APIHealthMonitoring.Infrastructure.HealthChecks.Services;

/// <summary>
/// Executes a single HTTP health check against a registered API endpoint.
/// Captures response time, status code, response size, and all error scenarios.
/// Never throws — all failures are captured as unsuccessful result objects.
/// </summary>
public class HealthCheckExecutor : IHealthCheckExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HealthCheckExecutor> _logger;

    public HealthCheckExecutor(IHttpClientFactory httpClientFactory, ILogger<HealthCheckExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<HealthCheck> ExecuteAsync(ApiEndpoint endpoint, CancellationToken ct = default)
    {
        var result = new HealthCheck
        {
            ApiEndpointId = endpoint.Id,
            CheckedAt     = DateTime.UtcNow,
        };

        // Build the full health check URL
        var healthUrl = BuildHealthUrl(endpoint.BaseUrl, endpoint.HealthEndpoint);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var client  = _httpClientFactory.CreateClient();
            client.Timeout    = TimeSpan.FromSeconds(endpoint.TimeoutSeconds);

            using var request = new HttpRequestMessage(
                MapHttpMethod(endpoint.HttpMethod), healthUrl);
            request.Headers.Add("User-Agent", "APIHealthMonitoring/1.0");

            using var response = await client.SendAsync(
                request, HttpCompletionOption.ResponseContentRead, ct);

            stopwatch.Stop();

            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.StatusCode     = (int)response.StatusCode;
            result.IsSuccessful   = result.StatusCode == endpoint.ExpectedStatusCode;

            // Capture response body size (content-length or actual read)
            if (response.Content.Headers.ContentLength.HasValue)
            {
                result.ResponseSizeBytes = response.Content.Headers.ContentLength.Value;
            }
            else if (endpoint.HttpMethod != Domain.Enums.HttpMethod.HEAD)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                result.ResponseSizeBytes = bytes.LongLength;
            }

            if (!result.IsSuccessful)
            {
                result.ErrorMessage = $"Unexpected status code {result.StatusCode} (expected {endpoint.ExpectedStatusCode})";
            }
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.IsSuccessful   = false;
            result.ErrorMessage   = "Request timed out";
            _logger.LogWarning("Health check timed out for endpoint '{Name}' ({Url})", endpoint.Name, healthUrl);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("Name or service not known")
                                           || ex.Message.Contains("No such host")
                                           || ex.Message.Contains("getaddrinfo"))
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.IsSuccessful   = false;
            result.ErrorMessage   = "DNS resolution failed";
            _logger.LogWarning("DNS failure for endpoint '{Name}': {Msg}", endpoint.Name, ex.Message);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("SSL") || ex.Message.Contains("certificate"))
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.IsSuccessful   = false;
            result.ErrorMessage   = "SSL certificate error";
            _logger.LogWarning("SSL error for endpoint '{Name}': {Msg}", endpoint.Name, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.IsSuccessful   = false;
            result.ErrorMessage   = $"Network error: {ex.Message}";
            _logger.LogWarning(ex, "Network error for endpoint '{Name}': {Msg}", endpoint.Name, ex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.IsSuccessful   = false;
            result.ErrorMessage   = "Invalid response received";
            _logger.LogError(ex, "Unexpected error checking endpoint '{Name}'", endpoint.Name);
        }

        return result;
    }

    private static string BuildHealthUrl(string baseUrl, string healthEndpoint)
    {
        if (string.IsNullOrWhiteSpace(healthEndpoint))
            return baseUrl.TrimEnd('/');

        if (healthEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            healthEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return healthEndpoint;
        }

        return baseUrl.TrimEnd('/') + "/" + healthEndpoint.TrimStart('/');
    }

    private static System.Net.Http.HttpMethod MapHttpMethod(Domain.Enums.HttpMethod method) =>
        method switch
        {
            Domain.Enums.HttpMethod.POST => System.Net.Http.HttpMethod.Post,
            Domain.Enums.HttpMethod.HEAD => System.Net.Http.HttpMethod.Head,
            _                            => System.Net.Http.HttpMethod.Get,
        };
}
