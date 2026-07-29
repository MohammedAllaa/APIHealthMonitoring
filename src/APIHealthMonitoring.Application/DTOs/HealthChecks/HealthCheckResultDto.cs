namespace APIHealthMonitoring.Application.DTOs.HealthChecks;

/// <summary>
/// Full response payload for a single health check execution record.
/// </summary>
public class HealthCheckResultDto
{
    public int Id { get; set; }
    public int ApiEndpointId { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public int ResponseTimeMs { get; set; }
    public int? StatusCode { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public long? ResponseSizeBytes { get; set; }
}
