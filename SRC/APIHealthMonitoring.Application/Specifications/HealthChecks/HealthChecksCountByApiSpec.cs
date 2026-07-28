using APIHealthMonitoring.Application.DTOs.HealthChecks;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.HealthChecks;

/// <summary>
/// COUNT companion to <see cref="HealthChecksByApiSpec"/> — same filters, no pagination.
/// </summary>
public class HealthChecksCountByApiSpec : BaseSpecification<HealthCheck>
{
    public HealthChecksCountByApiSpec(HealthCheckPagedRequestDto request)
    {
        AddCriteria(h =>
            (!request.ApiEndpointId.HasValue || h.ApiEndpointId == request.ApiEndpointId.Value) &&
            (!request.FromDate.HasValue      || h.CheckedAt >= request.FromDate.Value) &&
            (!request.ToDate.HasValue        || h.CheckedAt <= request.ToDate.Value) &&
            (!request.IsSuccessful.HasValue  || h.IsSuccessful == request.IsSuccessful.Value)
        );
    }
}
