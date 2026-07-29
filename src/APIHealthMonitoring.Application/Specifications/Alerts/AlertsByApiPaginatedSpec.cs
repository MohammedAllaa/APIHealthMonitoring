using APIHealthMonitoring.Application.DTOs.Alerts;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.Alerts;

/// <summary>
/// Paged, filterable alert list with optional filters on endpoint, severity, status, and date range.
/// </summary>
public class AlertsByApiPaginatedSpec : BaseSpecification<Alert>
{
    public AlertsByApiPaginatedSpec(AlertPagedRequestDto request)
    {
        AddCriteria(a =>
            (!request.ApiEndpointId.HasValue || a.ApiEndpointId == request.ApiEndpointId.Value) &&
            (!request.Severity.HasValue      || a.Severity == request.Severity.Value) &&
            (!request.Status.HasValue        || a.Status == request.Status.Value) &&
            (!request.FromDate.HasValue      || a.GeneratedAt >= request.FromDate.Value) &&
            (!request.ToDate.HasValue        || a.GeneratedAt <= request.ToDate.Value)
        );

        // Most recent alerts first
        ApplyOrderByDescending(a => a.GeneratedAt);

        var pageSize  = Math.Min(request.PageSize, 100);
        var pageIndex = Math.Max(request.PageIndex, 1);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
    }
}
