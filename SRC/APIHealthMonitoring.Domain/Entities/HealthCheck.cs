namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Stub entity for health check execution records.
/// Full implementation in Module 4.
/// </summary>
public class HealthCheck : BaseEntity
{
    /// <summary>The endpoint this check belongs to.</summary>
    public int ApiEndpointId { get; set; }

    /// <summary>Navigation back to the parent endpoint.</summary>
    public ApiEndpoint? ApiEndpoint { get; set; }
}
