using APIHealthMonitoring.Application.Interfaces.HealthChecks;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Infrastructure.HealthChecks.Services;

/// <summary>
/// Evaluates the health status of an API endpoint based on the latest check result
/// and the MonitoringConfiguration thresholds.
/// </summary>
public class HealthStatusEvaluator : IHealthStatusEvaluator
{
    /// <inheritdoc />
    public ApiHealthStatus Evaluate(
        HealthCheck result,
        MonitoringConfiguration config,
        int consecutiveFailures)
    {
        // If the request failed outright (network error, timeout, wrong status)
        if (!result.IsSuccessful)
        {
            // Exceeded failure limit → Critical
            return consecutiveFailures >= config.FailureCountLimit
                ? ApiHealthStatus.Critical
                : ApiHealthStatus.Warning;
        }

        // Request succeeded — evaluate by response time
        if (result.ResponseTimeMs >= config.CriticalThresholdMs)
            return ApiHealthStatus.Critical;

        if (result.ResponseTimeMs >= config.SlowThresholdMs)
            return ApiHealthStatus.Warning;

        return ApiHealthStatus.Healthy;
    }
}
