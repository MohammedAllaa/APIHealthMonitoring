using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Interfaces.Repositories;

/// <summary>
/// Extends the generic repository with ApiEndpoint-specific queries.
/// </summary>
public interface IApiEndpointRepository : IGenericRepository<ApiEndpoint>
{
    /// <summary>
    /// Checks whether an endpoint with the given name already exists.
    /// Used to enforce the uniqueness constraint at the service layer.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <param name="excludeId">
    /// Optional ID to exclude (used during updates so an endpoint can keep its own name).
    /// </param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<bool> NameExistsAsync(
        string name,
        int? excludeId = null,
        CancellationToken cancellationToken = default);
}
