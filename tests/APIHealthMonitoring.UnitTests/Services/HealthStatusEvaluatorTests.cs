using APIHealthMonitoring.Domain.Enums;
using APIHealthMonitoring.Infrastructure.HealthChecks.Services;
using APIHealthMonitoring.UnitTests.Common.Builders;
using FluentAssertions;

namespace APIHealthMonitoring.UnitTests.Services;

public class HealthStatusEvaluatorTests
{
    private readonly HealthStatusEvaluator _sut;

    public HealthStatusEvaluatorTests()
    {
        _sut = new HealthStatusEvaluator();
    }

    [Fact]
    public void Evaluate_ResponseTimeLessThanSlowThreshold_ShouldReturnHealthy()
    {
        // Arrange
        var config = new MonitoringConfigurationBuilder()
            .WithSlowThresholdMs(1000)
            .WithCriticalThresholdMs(2000)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(true)
            .WithResponseTimeMs(500)
            .Build();

        // Act
        var status = _sut.Evaluate(result, config, 0);

        // Assert
        status.Should().Be(ApiHealthStatus.Healthy);
    }

    [Fact]
    public void Evaluate_ResponseTimeBetweenSlowAndCriticalThresholds_ShouldReturnWarning()
    {
        // Arrange
        var config = new MonitoringConfigurationBuilder()
            .WithSlowThresholdMs(1000)
            .WithCriticalThresholdMs(2000)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(true)
            .WithResponseTimeMs(1500)
            .Build();

        // Act
        var status = _sut.Evaluate(result, config, 0);

        // Assert
        status.Should().Be(ApiHealthStatus.Warning);
    }

    [Fact]
    public void Evaluate_ResponseTimeGreaterThanCriticalThreshold_ShouldReturnCritical()
    {
        // Arrange
        var config = new MonitoringConfigurationBuilder()
            .WithSlowThresholdMs(1000)
            .WithCriticalThresholdMs(2000)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(true)
            .WithResponseTimeMs(2500)
            .Build();

        // Act
        var status = _sut.Evaluate(result, config, 0);

        // Assert
        status.Should().Be(ApiHealthStatus.Critical);
    }

    [Fact]
    public void Evaluate_UnsuccessfulCheckUnderFailureLimit_ShouldReturnWarning()
    {
        // Arrange
        var config = new MonitoringConfigurationBuilder()
            .WithFailureCountLimit(3)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(false)
            .Build();

        // Act
        var status = _sut.Evaluate(result, config, 2);

        // Assert
        status.Should().Be(ApiHealthStatus.Warning);
    }

    [Fact]
    public void Evaluate_UnsuccessfulCheckAtOrOverFailureLimit_ShouldReturnCritical()
    {
        // Arrange
        var config = new MonitoringConfigurationBuilder()
            .WithFailureCountLimit(3)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(false)
            .Build();

        // Act
        var status = _sut.Evaluate(result, config, 3);

        // Assert
        status.Should().Be(ApiHealthStatus.Critical);
    }

    [Fact]
    public void Evaluate_SuccessfulCheckWithZeroConsecutiveFailures_ShouldReturnHealthy()
    {
        // Arrange
        var config = new MonitoringConfigurationBuilder()
            .WithSlowThresholdMs(1000)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(true)
            .WithResponseTimeMs(100)
            .Build();

        // Act
        var status = _sut.Evaluate(result, config, 0);

        // Assert
        status.Should().Be(ApiHealthStatus.Healthy);
    }
}
