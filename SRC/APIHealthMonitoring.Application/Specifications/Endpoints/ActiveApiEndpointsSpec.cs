using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.Endpoints;

/// <summary>
/// Filters only endpoints where <c>IsActive = true</c>, ordered by name ascending.
/// Used by the monitoring engine to fetch the list of endpoints to poll.
/// </summary>
public class ActiveApiEndpointsSpec : BaseSpecification<ApiEndpoint>
{
    public ActiveApiEndpointsSpec()
        : base(e => e.IsActive)
    {
        ApplyOrderBy(e => e.Name);
    }
}
