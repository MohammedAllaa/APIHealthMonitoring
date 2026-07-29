using APIHealthMonitoring.Application.Constants;
using APIHealthMonitoring.Application.Interfaces.Alerts;
using APIHealthMonitoring.Application.Interfaces.HealthChecks;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;
using APIHealthMonitoring.Infrastructure.HealthChecks.Services;
using APIHealthMonitoring.UnitTests.Common.Builders;
using APIHealthMonitoring.UnitTests.Common.Fakes;
using FluentAssertions;
using Moq;

namespace APIHealthMonitoring.UnitTests.Services;

public class HealthCheckServiceTests
{
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly FakeCacheService _cacheService;
    private readonly Mock<IHealthCheckExecutor> _executorMock;
    private readonly Mock<IHealthStatusEvaluator> _evaluatorMock;
    private readonly Mock<IAlertEvaluator> _alertEvaluatorMock;
    private readonly HealthCheckService _sut;

    public HealthCheckServiceTests()
    {
        _unitOfWork = new FakeUnitOfWork();
        _cacheService = new FakeCacheService();
        _executorMock = new Mock<IHealthCheckExecutor>();
        _evaluatorMock = new Mock<IHealthStatusEvaluator>();
        _alertEvaluatorMock = new Mock<IAlertEvaluator>();

        _sut = new HealthCheckService(
            _unitOfWork,
            _executorMock.Object,
            _evaluatorMock.Object,
            _alertEvaluatorMock.Object,
            _cacheService
        );
    }

    [Fact]
    public async Task SaveResultAsync_SuccessfulCheck_ShouldResetConsecutiveFailuresAndSave()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder()
            .WithId(1)
            .WithConsecutiveFailures(4)
            .WithCurrentStatus(ApiHealthStatus.Critical)
            .Build();
        _unitOfWork.Repository<ApiEndpoint>().Add(endpoint);

        var config = new MonitoringConfigurationBuilder().WithApiEndpointId(1).Build();
        _unitOfWork.Repository<MonitoringConfiguration>().Add(config);

        var checkResult = new HealthCheckBuilder()
            .WithApiEndpointId(1)
            .WithIsSuccessful(true)
            .Build();

        _evaluatorMock.Setup(e => e.Evaluate(checkResult, config, 0))
            .Returns(ApiHealthStatus.Healthy);

        // Seed some fake cached items to check invalidation
        _cacheService.Set(CacheKeys.DashboardSummary, "value", TimeSpan.FromMinutes(1));
        _cacheService.Set($"{CacheKeys.DashboardApiCards}:1:10", "value", TimeSpan.FromMinutes(1));
        _cacheService.Set($"{CacheKeys.ApiStatsPrefix}1", "value", TimeSpan.FromMinutes(1));

        // Act
        await _sut.SaveResultAsync(checkResult, CancellationToken.None);

        // Assert
        // 1. Result persisted
        var savedChecks = await _unitOfWork.Repository<HealthCheck>().GetAllAsync();
        savedChecks.Should().ContainSingle();
        savedChecks.First().ApiEndpointId.Should().Be(1);
        savedChecks.First().IsSuccessful.Should().BeTrue();

        // 2. Counter reset
        var dbEndpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(1);
        dbEndpoint!.ConsecutiveFailures.Should().Be(0);
        dbEndpoint.CurrentStatus.Should().Be(ApiHealthStatus.Healthy);
        dbEndpoint.LastCheckedAt.Should().Be(checkResult.CheckedAt);

        // 3. Cache invalidated
        _cacheService.ContainsKey(CacheKeys.DashboardSummary).Should().BeFalse();
        _cacheService.ContainsKey($"{CacheKeys.DashboardApiCards}:1:10").Should().BeFalse();
        _cacheService.ContainsKey($"{CacheKeys.ApiStatsPrefix}1").Should().BeFalse();

        // 4. AlertEvaluator called
        _alertEvaluatorMock.Verify(a => a.EvaluateAndAlertAsync(dbEndpoint, checkResult, config, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveResultAsync_UnsuccessfulCheck_ShouldIncrementConsecutiveFailuresAndSave()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder()
            .WithId(1)
            .WithConsecutiveFailures(2)
            .WithCurrentStatus(ApiHealthStatus.Warning)
            .Build();
        _unitOfWork.Repository<ApiEndpoint>().Add(endpoint);

        var config = new MonitoringConfigurationBuilder().WithApiEndpointId(1).Build();
        _unitOfWork.Repository<MonitoringConfiguration>().Add(config);

        var checkResult = new HealthCheckBuilder()
            .WithApiEndpointId(1)
            .WithIsSuccessful(false)
            .Build();

        _evaluatorMock.Setup(e => e.Evaluate(checkResult, config, 3))
            .Returns(ApiHealthStatus.Critical);

        // Act
        await _sut.SaveResultAsync(checkResult, CancellationToken.None);

        // Assert
        var dbEndpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(1);
        dbEndpoint!.ConsecutiveFailures.Should().Be(3);
        dbEndpoint.CurrentStatus.Should().Be(ApiHealthStatus.Critical);
    }

    [Fact]
    public async Task TriggerManualCheckAsync_RateLimitedWithin10Seconds_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder().WithId(2).WithIsActive(true).Build();
        _unitOfWork.Repository<ApiEndpoint>().Add(endpoint);

        var checkResult = new HealthCheckBuilder().WithApiEndpointId(2).Build();
        _executorMock.Setup(e => e.ExecuteAsync(endpoint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkResult);

        // First trigger is successful
        await _sut.TriggerManualCheckAsync(2, CancellationToken.None);

        // Act
        Func<Task> act = async () => await _sut.TriggerManualCheckAsync(2, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*rate-limited*");
    }
}
