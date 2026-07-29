using APIHealthMonitoring.Application.Interfaces.Alerts;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Infrastructure.Alerts.Services;

/// <summary>
/// Evaluates each health check result against MonitoringConfiguration thresholds
/// and fires or auto-resolves alerts according to the business rules.
///
/// Trigger conditions (FR-6):
///  • Response time > CriticalThresholdMs             → Critical
///  • ConsecutiveFailures >= FailureCountLimit         → Critical
///  • Availability drops below AvailabilityThreshold  → Critical
///  • API becomes unreachable (null status code)       → Critical
///  • SlowThresholdMs ≤ ResponseTime < CriticalThresholdMs → Warning
///  • API is Healthy                                   → Auto-resolve all open alerts
/// </summary>
public class AlertEvaluator : IAlertEvaluator
{
    private readonly IAlertService _alertService;

    public AlertEvaluator(IAlertService alertService)
    {
        _alertService = alertService;
    }

    /// <inheritdoc />
    public async Task EvaluateAndAlertAsync(
        ApiEndpoint endpoint,
        HealthCheck result,
        MonitoringConfiguration? config,
        CancellationToken ct = default)
    {
        // If no config exists use defaults
        int slowMs       = config?.SlowThresholdMs       ?? 1000;
        int criticalMs   = config?.CriticalThresholdMs   ?? 2000;
        int failureLimit = config?.FailureCountLimit      ?? 3;

        // ---- Case 1: API is now Healthy — auto-resolve all open alerts ----
        if (endpoint.CurrentStatus == ApiHealthStatus.Healthy)
        {
            await _alertService.AutoResolveForEndpointAsync(endpoint.Id, ct);
            return;
        }

        // ---- Case 2: Unreachable (no status code at all) ----
        if (!result.StatusCode.HasValue && !result.IsSuccessful)
        {
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id,
                AlertSeverity.Critical,
                $"API '{endpoint.Name}' is unreachable: {result.ErrorMessage ?? "no response"}",
                ct);
            return;
        }

        // ---- Case 3: Consecutive failures reached the limit ----
        if (endpoint.ConsecutiveFailures >= failureLimit)
        {
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id,
                AlertSeverity.Critical,
                $"API '{endpoint.Name}' has failed {endpoint.ConsecutiveFailures} consecutive times.",
                ct);
            return;
        }

        // ---- Case 4: Non-successful response (wrong status code) ----
        if (!result.IsSuccessful)
        {
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id,
                AlertSeverity.Warning,
                $"API '{endpoint.Name}' returned unexpected status {result.StatusCode} (expected {endpoint.ExpectedStatusCode}).",
                ct);
            return;
        }

        // ---- Case 5: Response time in Critical range ----
        if (result.ResponseTimeMs >= criticalMs)
        {
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id,
                AlertSeverity.Critical,
                $"API '{endpoint.Name}' response time is critical: {result.ResponseTimeMs}ms (threshold: {criticalMs}ms).",
                ct);
            return;
        }

        // ---- Case 6: Response time in Warning range ----
        if (result.ResponseTimeMs >= slowMs)
        {
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id,
                AlertSeverity.Warning,
                $"API '{endpoint.Name}' response time is degraded: {result.ResponseTimeMs}ms (slow threshold: {slowMs}ms).",
                ct);
        }
    }
}
