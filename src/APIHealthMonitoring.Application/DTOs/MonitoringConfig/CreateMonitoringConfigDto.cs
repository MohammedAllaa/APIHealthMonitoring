using System.ComponentModel.DataAnnotations;

namespace APIHealthMonitoring.Application.DTOs.MonitoringConfig;

/// <summary>
/// Payload to create or initialize a custom monitoring configuration for an API endpoint.
/// </summary>
public class CreateMonitoringConfigDto : IValidatableObject
{
    [Required]
    public int ApiEndpointId { get; set; }

    [Range(100, 5000, ErrorMessage = "SlowThresholdMs must be between 100 and 5000 ms.")]
    public int SlowThresholdMs { get; set; } = 1000;

    [Range(500, 30000, ErrorMessage = "CriticalThresholdMs must be between 500 and 30000 ms.")]
    public int CriticalThresholdMs { get; set; } = 2000;

    [Range(1, 10, ErrorMessage = "FailureCountLimit must be between 1 and 10.")]
    public int FailureCountLimit { get; set; } = 3;

    [Range(50.0, 100.0, ErrorMessage = "AvailabilityThreshold must be between 50.0 and 100.0 percent.")]
    public decimal AvailabilityThreshold { get; set; } = 99.0m;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SlowThresholdMs >= CriticalThresholdMs)
        {
            yield return new ValidationResult(
                "SlowThresholdMs must be less than CriticalThresholdMs.",
                new[] { nameof(SlowThresholdMs), nameof(CriticalThresholdMs) });
        }
    }
}
