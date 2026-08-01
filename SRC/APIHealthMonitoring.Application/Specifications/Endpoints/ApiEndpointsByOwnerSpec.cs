using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.Endpoints;

/// <summary>
/// Filters endpoints by a partial, case-insensitive <c>ServiceOwner</c> match.
/// </summary>
public class ApiEndpointsByOwnerSpec : BaseSpecification<ApiEndpoint>
{
    public ApiEndpointsByOwnerSpec(string owner)
        : base(e => e.ServiceOwner.ToLower().Contains(owner.ToLower()))
    {
        ApplyOrderBy(e => e.Name);
    }
}
