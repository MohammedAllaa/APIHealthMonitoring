using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Endpoints;

/// <summary>
/// Lightweight summary for list/pagination views.
/// Returned by GET /api/endpoints (paged list).
/// </summary>
public class ApiEndpointSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Domain.Enums.Environment Environment { get; set; }
    public bool IsActive { get; set; }
    public string ServiceOwner { get; set; } = string.Empty;

    /// <summary>Latest health status (Healthy, Unhealthy, Degraded, Unknown).</summary>
    public string CurrentStatus { get; set; } = "Unknown";
}
