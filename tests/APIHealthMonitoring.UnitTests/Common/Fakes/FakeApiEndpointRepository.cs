using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.UnitTests.Common.Fakes;

/// <summary>
/// A list-backed Fake repository specifically for ApiEndpoint to support NameExistsAsync.
/// </summary>
public class FakeApiEndpointRepository : FakeRepository<ApiEndpoint>, IApiEndpointRepository
{
    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _entities.AsQueryable();
        
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        
        bool exists = query.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }
}
