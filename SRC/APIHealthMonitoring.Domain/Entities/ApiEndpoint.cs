using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Represents a monitored API endpoint registered in the system.
/// This is the core domain aggregate root for the API Registry module.
/// </summary>
public class ApiEndpoint : BaseEntity
{
    // -------------------------------------------------------------------------
    // Identity & Description
    // -------------------------------------------------------------------------

    /// <summary>Human-readable name for the endpoint. Must be unique.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The base URL of the service (e.g. https://api.myservice.com).</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The relative or absolute health check path
    /// (e.g. /health or https://api.myservice.com/health).
    /// </summary>
    public string HealthEndpoint { get; set; } = string.Empty;

    // -------------------------------------------------------------------------
    // Monitoring Configuration
    // -------------------------------------------------------------------------

    /// <summary>HTTP method used to call the health endpoint (GET, POST, HEAD).</summary>
    public Enums.HttpMethod HttpMethod { get; set; } = Enums.HttpMethod.GET;

    /// <summary>The HTTP status code that indicates a healthy response (e.g. 200).</summary>
    public int ExpectedStatusCode { get; set; } = 200;

    /// <summary>Seconds before the monitoring engine considers a request timed out (1–60).</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>How often the monitoring engine checks this endpoint (10–3600 seconds).</summary>
    public int IntervalSeconds { get; set; } = 60;

    // -------------------------------------------------------------------------
    // Ownership & Classification
    // -------------------------------------------------------------------------

    /// <summary>The team or individual responsible for this API.</summary>
    public string ServiceOwner { get; set; } = string.Empty;

    /// <summary>The deployment environment this endpoint belongs to.</summary>
    public Enums.Environment Environment { get; set; } = Enums.Environment.Development;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    /// <summary>When false the monitoring engine skips this endpoint.</summary>
    public bool IsActive { get; set; } = true;

    // -------------------------------------------------------------------------
    // Navigation Properties — populated by future modules
    // -------------------------------------------------------------------------

    /// <summary>
    /// Extended monitoring configuration (thresholds, retry policy, etc.).
    /// Navigation property — populated by Module 3.
    /// </summary>
    public MonitoringConfiguration? MonitoringConfig { get; set; }

    /// <summary>Health check execution records. Populated by Module 4.</summary>
    public ICollection<HealthCheck> HealthChecks { get; set; } = new List<HealthCheck>();

    /// <summary>Alert rules configured for this endpoint. Populated by Module 5.</summary>
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
