using APIHealthMonitoring.Application.DTOs.HealthChecks;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.HealthChecks;

/// <summary>
/// Count companion for HealthCheckSearchSpec.
/// </summary>
public class HealthCheckSearchCountSpec : BaseSpecification<HealthCheck>
{
    public HealthCheckSearchCountSpec(HealthCheckPagedRequestDto request)
    {
        AddCriteria(h =>
            (!request.ApiEndpointId.HasValue || h.ApiEndpointId == request.ApiEndpointId.Value) &&
            (!request.FromDate.HasValue || h.CheckedAt >= request.FromDate.Value) &&
            (!request.ToDate.HasValue || h.CheckedAt <= request.ToDate.Value) &&
            (!request.IsSuccessful.HasValue || h.IsSuccessful == request.IsSuccessful.Value)
        );
    }
}
