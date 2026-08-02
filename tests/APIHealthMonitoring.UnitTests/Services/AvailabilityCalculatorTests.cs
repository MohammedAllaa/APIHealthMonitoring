using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Infrastructure.Reporting.Services;
using APIHealthMonitoring.UnitTests.Common.Builders;
using FluentAssertions;

namespace APIHealthMonitoring.UnitTests.Services;

public class AvailabilityCalculatorTests
{
    private readonly AvailabilityCalculator _sut;

    public AvailabilityCalculatorTests()
    {
        _sut = new AvailabilityCalculator();
    }

    [Fact]
    public void Calculate_EmptyChecksList_ShouldReturnZeroes()
    {
        // Arrange
        var checks = Enumerable.Empty<HealthCheck>();

        // Act
        var result = _sut.Calculate(checks);

        // Assert
        result.AvailabilityPercentage.Should().Be(0m);
        result.AvgResponseTimeMs.Should().Be(0.0);
        result.TotalChecks.Should().Be(0);
        result.SuccessfulChecks.Should().Be(0);
        result.FailedChecks.Should().Be(0);
    }

    [Fact]
    public void Calculate_AllChecksSuccessful_ShouldReturn100PercentAvailability()
    {
        // Arrange
        var checks = new[]
        {
            new HealthCheckBuilder().WithIsSuccessful(true).WithResponseTimeMs(100).Build(),
            new HealthCheckBuilder().WithIsSuccessful(true).WithResponseTimeMs(200).Build(),
            new HealthCheckBuilder().WithIsSuccessful(true).WithResponseTimeMs(300).Build()
        };

        // Act
        var result = _sut.Calculate(checks);

        // Assert
        result.AvailabilityPercentage.Should().Be(100m);
        result.AvgResponseTimeMs.Should().Be(200.0); // Average of 100, 200, 300
        result.TotalChecks.Should().Be(3);
        result.SuccessfulChecks.Should().Be(3);
        result.FailedChecks.Should().Be(0);
    }

    [Fact]
    public void Calculate_AllChecksFailed_ShouldReturn0PercentAvailability()
    {
        // Arrange
        var checks = new[]
        {
            new HealthCheckBuilder().WithIsSuccessful(false).WithResponseTimeMs(0).Build(),
            new HealthCheckBuilder().WithIsSuccessful(false).WithResponseTimeMs(0).Build()
        };

        // Act
        var result = _sut.Calculate(checks);

        // Assert
        result.AvailabilityPercentage.Should().Be(0m);
        result.AvgResponseTimeMs.Should().Be(0.0);
        result.TotalChecks.Should().Be(2);
        result.SuccessfulChecks.Should().Be(0);
        result.FailedChecks.Should().Be(2);
    }

    [Fact]
    public void Calculate_HalfChecksSuccessful_ShouldReturn50PercentAvailability()
    {
        // Arrange
        var checks = new[]
        {
            new HealthCheckBuilder().WithIsSuccessful(true).WithResponseTimeMs(150).Build(),
            new HealthCheckBuilder().WithIsSuccessful(false).WithResponseTimeMs(0).Build()
        };

        // Act
        var result = _sut.Calculate(checks);

        // Assert
        result.AvailabilityPercentage.Should().Be(50m);
        result.AvgResponseTimeMs.Should().Be(150.0); // Only successful checks counted in average
        result.TotalChecks.Should().Be(2);
        result.SuccessfulChecks.Should().Be(1);
        result.FailedChecks.Should().Be(1);
    }

    [Fact]
    public void Calculate_WithVaryingTimesAndDates_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var checks = new[]
        {
            new HealthCheckBuilder().WithIsSuccessful(true).WithResponseTimeMs(120).WithCheckedAt(baseTime).Build(),
            new HealthCheckBuilder().WithIsSuccessful(true).WithResponseTimeMs(180).WithCheckedAt(baseTime.AddHours(-1)).Build(),
            new HealthCheckBuilder().WithIsSuccessful(false).WithResponseTimeMs(300).WithCheckedAt(baseTime.AddHours(-2)).Build()
        };

        // Act
        var result = _sut.Calculate(checks);

        // Assert
        result.AvailabilityPercentage.Should().Be(66.67m); // 2 success / 3 total = 66.666...%
        result.AvgResponseTimeMs.Should().Be(150.0); // (120 + 180) / 2
        result.TotalChecks.Should().Be(3);
        result.SuccessfulChecks.Should().Be(2);
        result.FailedChecks.Should().Be(1);
    }
}
