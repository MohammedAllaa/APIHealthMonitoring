namespace APIHealthMonitoring.Domain.Enums;

/// <summary>
/// Represents the deployment environment of a monitored API endpoint.
/// </summary>
public enum Environment
{
    Development = 0,
    QA          = 1,
    UAT         = 2,
    Production  = 3,
}
