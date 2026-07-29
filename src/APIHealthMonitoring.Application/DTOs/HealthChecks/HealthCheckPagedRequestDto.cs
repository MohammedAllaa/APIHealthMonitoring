namespace APIHealthMonitoring.Application.DTOs.HealthChecks;

/// <summary>
/// Query parameters for filtering the paged health check history list.
/// </summary>
public class HealthCheckPagedRequestDto
{
    /// <summary>Filter by a specific API endpoint ID. Null = all endpoints.</summary>
    public int? ApiEndpointId { get; set; }

    /// <summary>Page number, 1-based. Defaults to 1.</summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>Items per page. Defaults to 20, capped at 100.</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>Filter results on or after this UTC timestamp.</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>Filter results on or before this UTC timestamp.</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>Filter by success/failure. Null = all records.</summary>
    public bool? IsSuccessful { get; set; }
}
