using APIHealthMonitoring.Application.Interfaces.Reporting;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Infrastructure.Reporting.Services;

public class AvailabilityCalculator : IAvailabilityCalculator
{
    public (decimal AvailabilityPercentage, double AvgResponseTimeMs, int TotalChecks, int SuccessfulChecks, int FailedChecks) Calculate(IEnumerable<HealthCheck> checks)
    {
        var list = checks.ToList();
        if (!list.Any())
        {
            return (0m, 0.0, 0, 0, 0);
        }

        int total = list.Count;
        int success = list.Count(c => c.IsSuccessful);
        int failed = total - success;

        decimal availability = Math.Round((decimal)success / total * 100m, 2);

        var successfulChecks = list.Where(c => c.IsSuccessful).ToList();
        double avgResponseTime = successfulChecks.Any()
            ? Math.Round(successfulChecks.Average(c => c.ResponseTimeMs), 2)
            : 0.0;

        return (availability, avgResponseTime, total, success, failed);
    }
}
