using APIHealthMonitoring.Application.DTOs.Endpoints;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.Endpoints;

/// <summary>
/// Applies optional filters (name, environment, owner) and pagination
/// to the endpoint list query. Used by the paged list endpoint.
/// </summary>
public class ApiEndpointsPaginatedSpec : BaseSpecification<ApiEndpoint>
{
    public ApiEndpointsPaginatedSpec(ApiEndpointPagedRequestDto request)
    {
        // Build composite filter predicate
        AddCriteria(e =>
            (string.IsNullOrEmpty(request.Name)         || e.Name.ToLower().Contains(request.Name.ToLower())) &&
            (request.Environment == null                 || e.Environment == request.Environment) &&
            (string.IsNullOrEmpty(request.ServiceOwner) || e.ServiceOwner.ToLower().Contains(request.ServiceOwner.ToLower()))
        );

        // Default ordering — name ascending
        ApplyOrderBy(e => e.Name);

        // Enforce a max page size of 100
        var pageSize  = Math.Min(request.PageSize, 100);
        var pageIndex = Math.Max(request.PageIndex, 1);

        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
    }
}
