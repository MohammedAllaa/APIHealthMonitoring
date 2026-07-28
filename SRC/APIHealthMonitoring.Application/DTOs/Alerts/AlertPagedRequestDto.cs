using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Alerts;

/// <summary>Query parameters for filtering the paged alert list.</summary>
public class AlertPagedRequestDto
{
    /// <summary>Filter by a specific API endpoint. Null = all endpoints.</summary>
    public int? ApiEndpointId { get; set; }

    /// <summary>Filter by severity. Null = all severities.</summary>
    public AlertSeverity? Severity { get; set; }

    /// <summary>Filter by status (Open/Closed). Null = all statuses.</summary>
    public AlertStatus? Status { get; set; }

    /// <summary>Filter alerts generated on or after this UTC timestamp.</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>Filter alerts generated on or before this UTC timestamp.</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>Page number, 1-based. Defaults to 1.</summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>Items per page. Defaults to 20, capped at 100.</summary>
    public int PageSize { get; set; } = 20;
}
