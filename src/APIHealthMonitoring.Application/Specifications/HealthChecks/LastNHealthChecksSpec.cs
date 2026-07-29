using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.HealthChecks;

/// <summary>
/// Retrieves the last N health check results for a specific API endpoint, most recent first.
/// Used by the status evaluator to compute consecutive failure count.
/// </summary>
public class LastNHealthChecksSpec : BaseSpecification<HealthCheck>
{
    public LastNHealthChecksSpec(int apiEndpointId, int count)
        : base(h => h.ApiEndpointId == apiEndpointId)
    {
        ApplyOrderByDescending(h => h.CheckedAt);
        ApplyPaging(0, count);
    }
}
