using System.ComponentModel.DataAnnotations;

namespace APIHealthMonitoring.Application.DTOs.MonitoringConfig;

/// <summary>
/// Payload to update monitoring thresholds for an API endpoint.
/// </summary>
public class UpdateMonitoringConfigDto : IValidatableObject
{
    [Range(100, 5000, ErrorMessage = "SlowThresholdMs must be between 100 and 5000 ms.")]
    public int? SlowThresholdMs { get; set; }

    [Range(500, 30000, ErrorMessage = "CriticalThresholdMs must be between 500 and 30000 ms.")]
    public int? CriticalThresholdMs { get; set; }

    [Range(1, 10, ErrorMessage = "FailureCountLimit must be between 1 and 10.")]
    public int? FailureCountLimit { get; set; }

    [Range(50.0, 100.0, ErrorMessage = "AvailabilityThreshold must be between 50.0 and 100.0 percent.")]
    public decimal? AvailabilityThreshold { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SlowThresholdMs.HasValue && CriticalThresholdMs.HasValue && SlowThresholdMs.Value >= CriticalThresholdMs.Value)
        {
            yield return new ValidationResult(
                "SlowThresholdMs must be less than CriticalThresholdMs.",
                new[] { nameof(SlowThresholdMs), nameof(CriticalThresholdMs) });
        }
    }
}
