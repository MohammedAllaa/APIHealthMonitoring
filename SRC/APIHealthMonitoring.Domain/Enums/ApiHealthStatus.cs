namespace APIHealthMonitoring.Domain.Enums;

/// <summary>
/// Represents the evaluated health status of a monitored API endpoint.
/// Computed from the last N health check results and MonitoringConfiguration thresholds.
/// </summary>
public enum ApiHealthStatus
{
    Healthy  = 0,
    Warning  = 1,
    Critical = 2,
    Unknown  = 3,
}
