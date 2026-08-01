using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.Endpoints;

/// <summary>
/// Fetches a single <see cref="ApiEndpoint"/> by ID and eagerly loads
/// its <see cref="MonitoringConfiguration"/> navigation property.
/// Used when the service needs both records to build a full response.
/// </summary>
public class ApiEndpointByIdWithConfigSpec : BaseSpecification<ApiEndpoint>
{
    public ApiEndpointByIdWithConfigSpec(int id)
        : base(e => e.Id == id)
    {
        AddInclude(e => e.MonitoringConfig!);
    }
}
