using System.ComponentModel.DataAnnotations;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Endpoints;

/// <summary>
/// Payload to update an existing API endpoint registration.
/// All fields are optional — only provided fields are applied.
/// </summary>
public class UpdateApiEndpointDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [Url]
    [MaxLength(500)]
    public string? BaseUrl { get; set; }

    [MaxLength(500)]
    public string? HealthEndpoint { get; set; }

    public Domain.Enums.HttpMethod? HttpMethod { get; set; }

    [Range(100, 599, ErrorMessage = "ExpectedStatusCode must be a valid HTTP status code (100–599).")]
    public int? ExpectedStatusCode { get; set; }

    [Range(1, 60, ErrorMessage = "TimeoutSeconds must be between 1 and 60.")]
    public int? TimeoutSeconds { get; set; }

    [Range(10, 3600, ErrorMessage = "IntervalSeconds must be between 10 and 3600.")]
    public int? IntervalSeconds { get; set; }

    [MaxLength(200)]
    public string? ServiceOwner { get; set; }

    public Domain.Enums.Environment? Environment { get; set; }
}
