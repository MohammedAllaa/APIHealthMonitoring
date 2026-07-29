using APIHealthMonitoring.Application.DTOs.Endpoints;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.Specifications.Endpoints;

/// <summary>
/// Count companion for ApiEndpointSearchSpec.
/// </summary>
public class ApiEndpointSearchCountSpec : BaseSpecification<ApiEndpoint>
{
    public ApiEndpointSearchCountSpec(ApiEndpointPagedRequestDto request)
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
    }
}
