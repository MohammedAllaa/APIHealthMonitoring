using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Endpoints;

/// <summary>
/// Query parameters for the paginated endpoint list endpoint
/// GET /api/endpoints.
/// All filter fields are optional.
/// </summary>
public class ApiEndpointPagedRequestDto
{
    /// <summary>Page number, 1-based. Defaults to 1.</summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>Items per page. Defaults to 10, capped at 100.</summary>
    public int PageSize { get; set; } = 10;

    // -------------------------------------------------------------------------
    // Optional Filters
    // -------------------------------------------------------------------------

    /// <summary>Filter by partial name match (case-insensitive).</summary>
    public string? Name { get; set; }

    /// <summary>Filter by deployment environment.</summary>
    public Domain.Enums.Environment? Environment { get; set; }

    /// <summary>Filter by health status string (e.g. "Healthy", "Unhealthy").</summary>
    public string? Status { get; set; }

    /// <summary>Filter by partial service owner match (case-insensitive).</summary>
    public string? ServiceOwner { get; set; }
}
