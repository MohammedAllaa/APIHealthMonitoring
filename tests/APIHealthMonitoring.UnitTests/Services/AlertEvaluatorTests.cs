using APIHealthMonitoring.Application.Interfaces.Alerts;
using APIHealthMonitoring.Domain.Enums;
using APIHealthMonitoring.Infrastructure.Alerts.Services;
using APIHealthMonitoring.UnitTests.Common.Builders;
using Moq;

namespace APIHealthMonitoring.UnitTests.Services;

public class AlertEvaluatorTests
{
    private readonly Mock<IAlertService> _alertServiceMock;
    private readonly AlertEvaluator _sut;

    public AlertEvaluatorTests()
    {
        _alertServiceMock = new Mock<IAlertService>();
        _sut = new AlertEvaluator(_alertServiceMock.Object);
    }

    [Fact]
    public async Task EvaluateAndAlertAsync_EndpointIsHealthy_ShouldCallAutoResolveForEndpoint()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder()
            .WithId(10)
            .WithCurrentStatus(ApiHealthStatus.Healthy)
            .Build();

        var result = new HealthCheckBuilder().Build();
        var config = new MonitoringConfigurationBuilder().Build();

        // Act
        await _sut.EvaluateAndAlertAsync(endpoint, result, config, CancellationToken.None);

        // Assert
        _alertServiceMock.Verify(s => s.AutoResolveForEndpointAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        _alertServiceMock.Verify(s => s.CreateIfNotDuplicateAsync(It.IsAny<int>(), It.IsAny<AlertSeverity>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAndAlertAsync_UnreachableNoStatusCode_ShouldTriggerCriticalAlert()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder()
            .WithId(10)
            .WithCurrentStatus(ApiHealthStatus.Critical)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(false)
            .WithStatusCode(null)
            .WithErrorMessage("DNS resolution failed")
            .Build();

        var config = new MonitoringConfigurationBuilder().Build();

        // Act
        await _sut.EvaluateAndAlertAsync(endpoint, result, config, CancellationToken.None);

        // Assert
        _alertServiceMock.Verify(s => s.CreateIfNotDuplicateAsync(
            10,
            AlertSeverity.Critical,
            It.Is<string>(m => m.Contains("unreachable") && m.Contains("DNS resolution failed")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAndAlertAsync_ConsecutiveFailuresAtLimit_ShouldTriggerCriticalAlert()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder()
            .WithId(10)
            .WithCurrentStatus(ApiHealthStatus.Critical)
            .WithConsecutiveFailures(3)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(false)
            .WithStatusCode(500)
            .Build();

        var config = new MonitoringConfigurationBuilder()
            .WithFailureCountLimit(3)
            .Build();

        // Act
        await _sut.EvaluateAndAlertAsync(endpoint, result, config, CancellationToken.None);

        // Assert
        _alertServiceMock.Verify(s => s.CreateIfNotDuplicateAsync(
            10,
            AlertSeverity.Critical,
            It.Is<string>(m => m.Contains("failed 3 consecutive times")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAndAlertAsync_NonSuccessfulResponseWrongStatusCode_ShouldTriggerWarningAlert()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder()
            .WithId(10)
            .WithCurrentStatus(ApiHealthStatus.Warning)
            .WithConsecutiveFailures(1)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(false)
            .WithStatusCode(404)
            .Build();

        var config = new MonitoringConfigurationBuilder()
            .WithFailureCountLimit(3)
            .Build();

        // Act
        await _sut.EvaluateAndAlertAsync(endpoint, result, config, CancellationToken.None);

        // Assert
        _alertServiceMock.Verify(s => s.CreateIfNotDuplicateAsync(
            10,
            AlertSeverity.Warning,
            It.Is<string>(m => m.Contains("returned unexpected status 404")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAndAlertAsync_ResponseTimeInCriticalRange_ShouldTriggerCriticalAlert()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder()
            .WithId(10)
            .WithCurrentStatus(ApiHealthStatus.Critical)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(true)
            .WithResponseTimeMs(2500)
            .Build();

        var config = new MonitoringConfigurationBuilder()
            .WithSlowThresholdMs(1000)
            .WithCriticalThresholdMs(2000)
            .Build();

        // Act
        await _sut.EvaluateAndAlertAsync(endpoint, result, config, CancellationToken.None);

        // Assert
        _alertServiceMock.Verify(s => s.CreateIfNotDuplicateAsync(
            10,
            AlertSeverity.Critical,
            It.Is<string>(m => m.Contains("response time is critical: 2500ms")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAndAlertAsync_ResponseTimeInWarningRange_ShouldTriggerWarningAlert()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder()
            .WithId(10)
            .WithCurrentStatus(ApiHealthStatus.Warning)
            .Build();

        var result = new HealthCheckBuilder()
            .WithIsSuccessful(true)
            .WithResponseTimeMs(1500)
            .Build();

        var config = new MonitoringConfigurationBuilder()
            .WithSlowThresholdMs(1000)
            .WithCriticalThresholdMs(2000)
            .Build();

        // Act
        await _sut.EvaluateAndAlertAsync(endpoint, result, config, CancellationToken.None);

        // Assert
        _alertServiceMock.Verify(s => s.CreateIfNotDuplicateAsync(
            10,
            AlertSeverity.Warning,
            It.Is<string>(m => m.Contains("response time is degraded: 1500ms")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
