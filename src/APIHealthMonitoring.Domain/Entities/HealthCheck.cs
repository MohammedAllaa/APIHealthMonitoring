namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Represents a single health check execution result for a monitored API endpoint.
/// Records are immutable once written — no updates or deletes.
/// </summary>
public class HealthCheck : BaseEntity
{
    // -------------------------------------------------------------------------
    // FK & Navigation
    // -------------------------------------------------------------------------

    /// <summary>The endpoint this check was executed against.</summary>
    public int ApiEndpointId { get; set; }

    /// <summary>Navigation back to the parent endpoint.</summary>
    public ApiEndpoint? ApiEndpoint { get; set; }

    // -------------------------------------------------------------------------
    // Execution Data
    // -------------------------------------------------------------------------

    /// <summary>UTC timestamp when this check was executed.</summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>Round-trip time from request initiation to response received (milliseconds).</summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>HTTP status code returned by the endpoint. Null if request timed out or network failure.</summary>
    public int? StatusCode { get; set; }

    /// <summary>True if the response met both the expected status code and timeout thresholds.</summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Descriptive error message when <see cref="IsSuccessful"/> is false.
    /// Examples: "Request timed out", "DNS resolution failed", "SSL certificate error".
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Size of the response body in bytes. Null if not available (e.g. HEAD requests or failures).</summary>
    public long? ResponseSizeBytes { get; set; }
}
