using APIHealthMonitoring.Application.DTOs.Endpoints;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.Endpoints;

/// <summary>
/// COUNT companion to <see cref="ApiEndpointsPaginatedSpec"/>.
/// Applies the same filters WITHOUT ordering or pagination
/// so the repository can execute an efficient COUNT query for total pages.
/// </summary>
public class ApiEndpointsCountSpec : BaseSpecification<ApiEndpoint>
{
    public ApiEndpointsCountSpec(ApiEndpointPagedRequestDto request)
    {
        AddCriteria(e =>
            (string.IsNullOrEmpty(request.Name)         || e.Name.ToLower().Contains(request.Name.ToLower())) &&
            (request.Environment == null                 || e.Environment == request.Environment) &&
            (string.IsNullOrEmpty(request.ServiceOwner) || e.ServiceOwner.ToLower().Contains(request.ServiceOwner.ToLower()))
        );
        // No ordering, no paging — COUNT only
    }
}
