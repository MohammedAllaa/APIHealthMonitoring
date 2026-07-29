using APIHealthMonitoring.Application.DTOs.Endpoints;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.Specifications.Endpoints;

/// <summary>
/// Specification for searching and filtering API endpoints.
/// </summary>
public class ApiEndpointSearchSpec : BaseSpecification<ApiEndpoint>
{
    public ApiEndpointSearchSpec(ApiEndpointPagedRequestDto request)
    {
        ApiHealthStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ApiHealthStatus>(request.Status, true, out var statusVal))
        {
            parsedStatus = statusVal;
        }

        AddCriteria(e =>
            (string.IsNullOrEmpty(request.Name) || e.Name.ToLower().Contains(request.Name.ToLower())) &&
            (request.Environment == null || e.Environment == request.Environment) &&
            (string.IsNullOrEmpty(request.ServiceOwner) || e.ServiceOwner.ToLower().Contains(request.ServiceOwner.ToLower())) &&
            (parsedStatus == null || e.CurrentStatus == parsedStatus.Value)
        );

        ApplyOrderBy(e => e.Name);

        var pageSize = Math.Min(request.PageSize, 100);
        var pageIndex = Math.Max(request.PageIndex, 1);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
    }
}
