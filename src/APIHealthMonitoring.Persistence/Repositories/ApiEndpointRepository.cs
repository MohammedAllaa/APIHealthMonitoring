using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace APIHealthMonitoring.Persistence.Repositories;

/// <summary>
/// Concrete repository for <see cref="ApiEndpoint"/>.
/// Inherits all generic operations from <see cref="GenericRepository{T}"/>
/// and adds the endpoint-specific name-uniqueness check.
/// </summary>
public class ApiEndpointRepository : GenericRepository<ApiEndpoint>, IApiEndpointRepository
{
    public ApiEndpointRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<bool> NameExistsAsync(
        string name,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(e =>
                e.Name.ToLower() == name.ToLower() &&
                (excludeId == null || e.Id != excludeId.Value),
                cancellationToken);
    }
}
