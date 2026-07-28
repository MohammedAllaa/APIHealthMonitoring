namespace APIHealthMonitoring.Domain.Enums;

/// <summary>
/// The HTTP method used when the monitoring engine calls the health endpoint.
/// </summary>
public enum HttpMethod
{
    GET  = 0,
    POST = 1,
    HEAD = 2,
}
