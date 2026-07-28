using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.Specifications.Alerts;

/// <summary>
/// Finds an existing open alert for a specific API endpoint and severity.
/// Used for deduplication — prevents creating duplicate active alerts.
/// </summary>
public class OpenAlertsByApiSpec : BaseSpecification<Alert>
{
    public OpenAlertsByApiSpec(int apiEndpointId, AlertSeverity severity)
        : base(a => a.ApiEndpointId == apiEndpointId
                 && a.Severity == severity
                 && a.Status == AlertStatus.Open)
    {
    }
}
