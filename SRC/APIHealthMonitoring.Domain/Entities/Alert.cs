namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Stub entity for alert rules configured for an endpoint.
/// Full implementation in Module 5.
/// </summary>
public class Alert : BaseEntity
{
    /// <summary>The endpoint this alert belongs to.</summary>
    public int ApiEndpointId { get; set; }

    /// <summary>Navigation back to the parent endpoint.</summary>
    public ApiEndpoint? ApiEndpoint { get; set; }
}
