using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.Specifications.Alerts;

/// <summary>
/// Gets all open alerts for a specific API endpoint.
/// Used by auto-resolution when the endpoint recovers to Healthy.
/// </summary>
public class OpenAlertsByEndpointSpec : BaseSpecification<Alert>
{
    public OpenAlertsByEndpointSpec(int apiEndpointId)
        : base(a => a.ApiEndpointId == apiEndpointId && a.Status == AlertStatus.Open)
    {
        ApplyOrderByDescending(a => a.GeneratedAt);
    }
}
