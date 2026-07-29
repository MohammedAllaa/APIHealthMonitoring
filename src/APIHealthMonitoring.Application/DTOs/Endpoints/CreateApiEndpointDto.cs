using System.ComponentModel.DataAnnotations;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Endpoints;

/// <summary>
/// Payload to register a new API endpoint for monitoring.
/// </summary>
public class CreateApiEndpointDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Url]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string HealthEndpoint { get; set; } = string.Empty;

    [Required]
    public Domain.Enums.HttpMethod HttpMethod { get; set; } = Domain.Enums.HttpMethod.GET;

    [Range(100, 599, ErrorMessage = "ExpectedStatusCode must be a valid HTTP status code (100–599).")]
    public int ExpectedStatusCode { get; set; } = 200;

    [Range(1, 60, ErrorMessage = "TimeoutSeconds must be between 1 and 60.")]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(10, 3600, ErrorMessage = "IntervalSeconds must be between 10 and 3600.")]
    public int IntervalSeconds { get; set; } = 60;

    [Required]
    [MaxLength(200)]
    public string ServiceOwner { get; set; } = string.Empty;

    [Required]
    public Domain.Enums.Environment Environment { get; set; } = Domain.Enums.Environment.Development;
}
