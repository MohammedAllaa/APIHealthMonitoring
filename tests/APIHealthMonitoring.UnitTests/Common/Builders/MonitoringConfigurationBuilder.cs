using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.UnitTests.Common.Builders;

public class MonitoringConfigurationBuilder
{
    private int _id = 1;
    private int _apiEndpointId = 1;
    private int _slowThresholdMs = 1000;
    private int _criticalThresholdMs = 2000;
    private int _failureCountLimit = 3;
    private decimal _availabilityThreshold = 99.0m;

    public MonitoringConfigurationBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public MonitoringConfigurationBuilder WithApiEndpointId(int apiEndpointId)
    {
        _apiEndpointId = apiEndpointId;
        return this;
    }

    public MonitoringConfigurationBuilder WithSlowThresholdMs(int slowThresholdMs)
    {
        _slowThresholdMs = slowThresholdMs;
        return this;
    }

    public MonitoringConfigurationBuilder WithCriticalThresholdMs(int criticalThresholdMs)
    {
        _criticalThresholdMs = criticalThresholdMs;
        return this;
    }

    public MonitoringConfigurationBuilder WithFailureCountLimit(int failureCountLimit)
    {
        _failureCountLimit = failureCountLimit;
        return this;
    }

    public MonitoringConfigurationBuilder WithAvailabilityThreshold(decimal availabilityThreshold)
    {
        _availabilityThreshold = availabilityThreshold;
        return this;
    }

    public MonitoringConfiguration Build()
    {
        return new MonitoringConfiguration
        {
            Id = _id,
            ApiEndpointId = _apiEndpointId,
            SlowThresholdMs = _slowThresholdMs,
            CriticalThresholdMs = _criticalThresholdMs,
            FailureCountLimit = _failureCountLimit,
            AvailabilityThreshold = _availabilityThreshold,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
