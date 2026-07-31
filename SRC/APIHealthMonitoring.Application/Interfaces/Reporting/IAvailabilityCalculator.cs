using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Interfaces.Reporting;

public interface IAvailabilityCalculator
{
    /// <summary>
    /// Computes success rate percentage and average response time from a list of health check records.
    /// </summary>
    (decimal AvailabilityPercentage, double AvgResponseTimeMs, int TotalChecks, int SuccessfulChecks, int FailedChecks) Calculate(IEnumerable<HealthCheck> checks);
}
