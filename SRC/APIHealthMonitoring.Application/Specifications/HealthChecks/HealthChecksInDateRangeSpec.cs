using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.HealthChecks;

/// <summary>
/// Retrieves health checks in a specified date range, ordered most recent first.
/// Used for date-range-filtered reports.
/// </summary>
public class HealthChecksInDateRangeSpec : BaseSpecification<HealthCheck>
{
    public HealthChecksInDateRangeSpec(int apiEndpointId, DateTime from, DateTime to)
        : base(h => h.ApiEndpointId == apiEndpointId && h.CheckedAt >= from && h.CheckedAt <= to)
    {
        ApplyOrderByDescending(h => h.CheckedAt);
    }
}
