using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Specifications.MonitoringConfig;

/// <summary>
/// Fetches the <see cref="MonitoringConfiguration"/> for a given <c>ApiEndpointId</c>
/// and eagerly includes the associated <see cref="ApiEndpoint"/>.
/// </summary>
public class ConfigByApiEndpointIdSpec : BaseSpecification<MonitoringConfiguration>
{
    public ConfigByApiEndpointIdSpec(int apiEndpointId)
        : base(c => c.ApiEndpointId == apiEndpointId)
    {
        AddInclude(c => c.ApiEndpoint!);
    }
}
