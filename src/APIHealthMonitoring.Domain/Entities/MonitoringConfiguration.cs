namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Defines performance and health thresholds for a single monitored API endpoint.
/// Has a 1:1 relationship with <see cref="ApiEndpoint"/>.
/// </summary>
public class MonitoringConfiguration : BaseEntity
{
    /// <summary>The endpoint this configuration applies to (FK, Unique).</summary>
    public int ApiEndpointId { get; set; }

    /// <summary>Response time threshold in milliseconds above which request is marked Slow/Degraded (100–5000ms).</summary>
    public int SlowThresholdMs { get; set; } = 1000;

    /// <summary>Response time threshold in milliseconds above which request is marked Critical (500–30000ms).</summary>
    public int CriticalThresholdMs { get; set; } = 2000;

    /// <summary>Consecutive failure count threshold before triggering Unhealthy state (1–10).</summary>
    public int FailureCountLimit { get; set; } = 3;

    /// <summary>Target SLA availability percentage (50.0–100.0%).</summary>
    public decimal AvailabilityThreshold { get; set; } = 99.0m;

    /// <summary>Navigation back to parent endpoint.</summary>
    public ApiEndpoint? ApiEndpoint { get; set; }
}
