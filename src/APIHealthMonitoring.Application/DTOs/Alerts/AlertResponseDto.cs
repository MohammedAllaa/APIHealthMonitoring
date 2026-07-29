using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Alerts;

/// <summary>Response DTO for a single alert record.</summary>
public class AlertResponseDto
{
    public int AlertId { get; set; }
    public int ApiEndpointId { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public AlertStatus Status { get; set; }
}
