using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Endpoints;

/// <summary>
/// Full detail response for a single API endpoint.
/// Returned by GET /api/endpoints/{id}.
/// </summary>
public class ApiEndpointResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string HealthEndpoint { get; set; } = string.Empty;
    public Domain.Enums.HttpMethod HttpMethod { get; set; }
    public int ExpectedStatusCode { get; set; }
    public int TimeoutSeconds { get; set; }
    public int IntervalSeconds { get; set; }
    public string ServiceOwner { get; set; } = string.Empty;
    public Domain.Enums.Environment Environment { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // -------------------------------------------------------------------------
    // Runtime status — populated by the monitoring engine (Module 4)
    // -------------------------------------------------------------------------

    /// <summary>Latest health status (Healthy, Unhealthy, Degraded, Unknown).</summary>
    public string CurrentStatus { get; set; } = "Unknown";

    /// <summary>UTC timestamp of the most recent health check execution.</summary>
    public DateTime? LastCheckedAt { get; set; }
}
