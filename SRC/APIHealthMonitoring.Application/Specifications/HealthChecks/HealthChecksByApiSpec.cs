using APIHealthMonitoring.Application.DTOs.HealthChecks;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.HealthChecks;

/// <summary>
/// Paged health check history for a specific API endpoint with optional filters.
/// </summary>
public class HealthChecksByApiSpec : BaseSpecification<HealthCheck>
{
    public HealthChecksByApiSpec(HealthCheckPagedRequestDto request)
    {
        AddCriteria(h =>
            (!request.ApiEndpointId.HasValue || h.ApiEndpointId == request.ApiEndpointId.Value) &&
            (!request.FromDate.HasValue      || h.CheckedAt >= request.FromDate.Value) &&
            (!request.ToDate.HasValue        || h.CheckedAt <= request.ToDate.Value) &&
            (!request.IsSuccessful.HasValue  || h.IsSuccessful == request.IsSuccessful.Value)
        );

        // Most recent checks first
        ApplyOrderByDescending(h => h.CheckedAt);

        var pageSize  = Math.Min(request.PageSize, 100);
        var pageIndex = Math.Max(request.PageIndex, 1);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
    }
}
