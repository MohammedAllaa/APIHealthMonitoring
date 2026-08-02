using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.Interfaces.HealthChecks;

/// <summary>
/// Evaluates the current health status of an API endpoint
/// based on MonitoringConfiguration thresholds and the latest check results.
/// </summary>
public interface IHealthStatusEvaluator
{
    /// <summary>
    /// Derives a <see cref="ApiHealthStatus"/> value from a single health check result
    /// and the endpoint's configuration thresholds.
    /// </summary>
    ApiHealthStatus Evaluate(HealthCheck result, MonitoringConfiguration config, int consecutiveFailures);
}
