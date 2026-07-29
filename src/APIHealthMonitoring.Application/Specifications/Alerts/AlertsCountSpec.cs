using APIHealthMonitoring.Application.DTOs.Alerts;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.Alerts;

/// <summary>
/// COUNT companion to <see cref="AlertsByApiPaginatedSpec"/> — same filters, no pagination.
/// </summary>
public class AlertsCountSpec : BaseSpecification<Alert>
{
    public AlertsCountSpec(AlertPagedRequestDto request)
    {
        AddCriteria(a =>
            (!request.ApiEndpointId.HasValue || a.ApiEndpointId == request.ApiEndpointId.Value) &&
            (!request.Severity.HasValue      || a.Severity == request.Severity.Value) &&
            (!request.Status.HasValue        || a.Status == request.Status.Value) &&
            (!request.FromDate.HasValue      || a.GeneratedAt >= request.FromDate.Value) &&
            (!request.ToDate.HasValue        || a.GeneratedAt <= request.ToDate.Value)
        );
    }
}
