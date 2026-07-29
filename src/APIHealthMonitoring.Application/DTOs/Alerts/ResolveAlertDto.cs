namespace APIHealthMonitoring.Application.DTOs.Alerts;

/// <summary>Request body for resolving an open alert.</summary>
public class ResolveAlertDto
{
    /// <summary>
    /// UTC timestamp to record as the resolution time.
    /// Defaults to <see cref="DateTime.UtcNow"/> when not provided.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
}
