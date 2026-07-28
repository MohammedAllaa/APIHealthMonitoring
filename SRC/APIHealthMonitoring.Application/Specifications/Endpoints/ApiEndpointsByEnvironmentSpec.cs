using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.Specifications.Endpoints;

/// <summary>
/// Filters endpoints by their deployment <see cref="Domain.Enums.Environment"/>, ordered by name.
/// </summary>
public class ApiEndpointsByEnvironmentSpec : BaseSpecification<ApiEndpoint>
{
    public ApiEndpointsByEnvironmentSpec(Domain.Enums.Environment environment)
        : base(e => e.Environment == environment)
    {
        ApplyOrderBy(e => e.Name);
    }
}
