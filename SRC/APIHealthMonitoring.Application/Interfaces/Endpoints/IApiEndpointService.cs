using APIHealthMonitoring.Application.DTOs.Endpoints;
using APIHealthMonitoring.Application.Specifications;

namespace APIHealthMonitoring.Application.Interfaces.Endpoints;

/// <summary>
/// Defines CRUD and lifecycle operations for registered API endpoints.
/// </summary>
public interface IApiEndpointService
{
    /// <summary>
    /// Registers a new API endpoint for monitoring.
    /// Validates name uniqueness before creation.
    /// </summary>
    Task<ApiEndpointResponseDto> CreateAsync(CreateApiEndpointDto request, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated, filtered list of all registered endpoints.
    /// </summary>
    Task<PaginatedResult<ApiEndpointSummaryDto>> GetPagedAsync(ApiEndpointPagedRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Returns full details for a single endpoint by ID.
    /// Throws <see cref="KeyNotFoundException"/> when not found.
    /// </summary>
    Task<ApiEndpointResponseDto> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Applies partial updates to an existing endpoint.
    /// Validates name uniqueness when name changes.
    /// </summary>
    Task<ApiEndpointResponseDto> UpdateAsync(int id, UpdateApiEndpointDto request, CancellationToken ct = default);

    /// <summary>
    /// Permanently removes an endpoint from the registry.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Sets <c>IsActive = true</c> on the endpoint.</summary>
    Task ActivateAsync(int id, CancellationToken ct = default);

    /// <summary>Sets <c>IsActive = false</c> on the endpoint.</summary>
    Task DeactivateAsync(int id, CancellationToken ct = default);
}
