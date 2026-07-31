using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.HealthChecks;

/// <summary>
/// Current health status summary for a single API endpoint.
/// Returned by GET /api/endpoints/{id}/status.
/// </summary>
public class ApiHealthSummaryDto
{
    public int ApiEndpointId { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public ApiHealthStatus CurrentStatus { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public int ConsecutiveFailures { get; set; }

    /// <summary>Availability percentage for the current UTC calendar day (0.0–100.0).</summary>
    public decimal TodayAvailability { get; set; }
}
