using APIHealthMonitoring.Application.DTOs.HealthChecks;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.HealthChecks;

/// <summary>
/// Specification for searching and filtering Health Check history.
/// </summary>
public class HealthCheckSearchSpec : BaseSpecification<HealthCheck>
{
    public HealthCheckSearchSpec(HealthCheckPagedRequestDto request)
    {
        AddCriteria(h =>
            (!request.ApiEndpointId.HasValue || h.ApiEndpointId == request.ApiEndpointId.Value) &&
            (!request.FromDate.HasValue || h.CheckedAt >= request.FromDate.Value) &&
            (!request.ToDate.HasValue || h.CheckedAt <= request.ToDate.Value) &&
            (!request.IsSuccessful.HasValue || h.IsSuccessful == request.IsSuccessful.Value)
        );

        ApplyOrderByDescending(h => h.CheckedAt);

        var pageSize = Math.Min(request.PageSize, 100);
        var pageIndex = Math.Max(request.PageIndex, 1);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
    }
}
