namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Stub entity for extended monitoring configuration (thresholds, retry policy, etc.).
/// Full implementation in Module 3.
/// </summary>
public class MonitoringConfiguration : BaseEntity
{
    /// <summary>The endpoint this config belongs to.</summary>
    public int ApiEndpointId { get; set; }

    /// <summary>Navigation back to the parent endpoint.</summary>
    public ApiEndpoint? ApiEndpoint { get; set; }
}
