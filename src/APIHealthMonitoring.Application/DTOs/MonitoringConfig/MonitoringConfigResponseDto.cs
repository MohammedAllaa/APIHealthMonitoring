namespace APIHealthMonitoring.Application.DTOs.MonitoringConfig;

/// <summary>
/// Response payload containing full monitoring configuration details.
/// </summary>
public class MonitoringConfigResponseDto
{
    public int Id { get; set; }
    public int ApiEndpointId { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public int SlowThresholdMs { get; set; }
    public int CriticalThresholdMs { get; set; }
    public int FailureCountLimit { get; set; }
    public decimal AvailabilityThreshold { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
