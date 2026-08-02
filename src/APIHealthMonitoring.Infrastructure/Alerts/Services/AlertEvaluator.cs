using APIHealthMonitoring.Application.Interfaces.Alerts;
using APIHealthMonitoring.Application.Interfaces.Notifications;
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
///
/// After each alert event a Slack webhook notification is dispatched
/// via <see cref="ISlackNotificationService"/> (failures are swallowed — never break monitoring).
/// </summary>
public class AlertEvaluator : IAlertEvaluator
{
    private readonly IAlertService             _alertService;
    private readonly ISlackNotificationService _slack;

    public AlertEvaluator(
        IAlertService             alertService,
        ISlackNotificationService slack)
    {
        _alertService = alertService;
        _slack        = slack;
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
            await _slack.SendAlertNotificationAsync(
                endpoint.Name,
                AlertSeverity.Warning,   // severity is ignored when isResolved = true
                $"API '{endpoint.Name}' has recovered and is now Healthy.",
                isResolved: true,
                ct);
            return;
        }

        // ---- Case 2: Unreachable (no status code at all) ----
        if (!result.StatusCode.HasValue && !result.IsSuccessful)
        {
            var msg = $"API '{endpoint.Name}' is unreachable: {result.ErrorMessage ?? "no response"}";
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id, AlertSeverity.Critical, msg, ct);
            await _slack.SendAlertNotificationAsync(
                endpoint.Name, AlertSeverity.Critical, msg, isResolved: false, ct);
            return;
        }

        // ---- Case 3: Consecutive failures reached the limit ----
        if (endpoint.ConsecutiveFailures >= failureLimit)
        {
            var msg = $"API '{endpoint.Name}' has failed {endpoint.ConsecutiveFailures} consecutive times.";
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id, AlertSeverity.Critical, msg, ct);
            await _slack.SendAlertNotificationAsync(
                endpoint.Name, AlertSeverity.Critical, msg, isResolved: false, ct);
            return;
        }

        // ---- Case 4: Non-successful response (wrong status code) ----
        if (!result.IsSuccessful)
        {
            var msg = $"API '{endpoint.Name}' returned unexpected status {result.StatusCode} (expected {endpoint.ExpectedStatusCode}).";
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id, AlertSeverity.Warning, msg, ct);
            await _slack.SendAlertNotificationAsync(
                endpoint.Name, AlertSeverity.Warning, msg, isResolved: false, ct);
            return;
        }

        // ---- Case 5: Response time in Critical range ----
        if (result.ResponseTimeMs >= criticalMs)
        {
            var msg = $"API '{endpoint.Name}' response time is critical: {result.ResponseTimeMs}ms (threshold: {criticalMs}ms).";
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id, AlertSeverity.Critical, msg, ct);
            await _slack.SendAlertNotificationAsync(
                endpoint.Name, AlertSeverity.Critical, msg, isResolved: false, ct);
            return;
        }

        // ---- Case 6: Response time in Warning range ----
        if (result.ResponseTimeMs >= slowMs)
        {
            var msg = $"API '{endpoint.Name}' response time is degraded: {result.ResponseTimeMs}ms (slow threshold: {slowMs}ms).";
            await _alertService.CreateIfNotDuplicateAsync(
                endpoint.Id, AlertSeverity.Warning, msg, ct);
            await _slack.SendAlertNotificationAsync(
                endpoint.Name, AlertSeverity.Warning, msg, isResolved: false, ct);
        }
    }
}
