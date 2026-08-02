using APIHealthMonitoring.Application.DTOs.Alerts;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;
using APIHealthMonitoring.Infrastructure.Alerts.Services;
using APIHealthMonitoring.UnitTests.Common.Fakes;
using FluentAssertions;

namespace APIHealthMonitoring.UnitTests.Services;

public class AlertServiceTests
{
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly FakeCacheService _cacheService;
    private readonly AlertService _sut;

    public AlertServiceTests()
    {
        _unitOfWork = new FakeUnitOfWork();
        _cacheService = new FakeCacheService();
        _sut = new AlertService(_unitOfWork, _cacheService, new Microsoft.Extensions.Logging.Abstractions.NullLogger<AlertService>());
    }

    [Fact]
    public async Task CreateIfNotDuplicateAsync_NonDuplicateAlert_ShouldCreateAlertInDatabase()
    {
        // Arrange
        int apiEndpointId = 1;
        var severity = AlertSeverity.Critical;
        var message = "API down";

        // Act
        await _sut.CreateIfNotDuplicateAsync(apiEndpointId, severity, message, CancellationToken.None);

        // Assert
        var alerts = await _unitOfWork.Repository<Alert>().GetAllAsync();
        alerts.Should().ContainSingle();
        alerts.First().ApiEndpointId.Should().Be(apiEndpointId);
        alerts.First().Severity.Should().Be(severity);
        alerts.First().Message.Should().Be(message);
        alerts.First().Status.Should().Be(AlertStatus.Open);
    }

    [Fact]
    public async Task CreateIfNotDuplicateAsync_DuplicateOpenAlert_ShouldNotCreateAlert()
    {
        // Arrange
        int apiEndpointId = 1;
        var severity = AlertSeverity.Critical;
        
        // Seed an existing open alert
        var existingAlert = new Alert
        {
            ApiEndpointId = apiEndpointId,
            Severity = severity,
            Message = "Original alert",
            Status = AlertStatus.Open,
            GeneratedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<Alert>().Add(existingAlert);

        // Act
        await _sut.CreateIfNotDuplicateAsync(apiEndpointId, severity, "Duplicate alert", CancellationToken.None);

        // Assert
        var alerts = await _unitOfWork.Repository<Alert>().GetAllAsync();
        alerts.Should().HaveCount(1);
        alerts.First().Message.Should().Be("Original alert");
    }

    [Fact]
    public async Task AutoResolveForEndpointAsync_OpenAlertsExist_ShouldChangeStatusToClosedAndSetResolvedAt()
    {
        // Arrange
        int apiEndpointId = 1;
        var alert = new Alert
        {
            ApiEndpointId = apiEndpointId,
            Severity = AlertSeverity.Warning,
            Message = "Alert message",
            Status = AlertStatus.Open,
            GeneratedAt = DateTime.UtcNow.AddHours(-1)
        };
        _unitOfWork.Repository<Alert>().Add(alert);

        // Act
        await _sut.AutoResolveForEndpointAsync(apiEndpointId, CancellationToken.None);

        // Assert
        var alerts = await _unitOfWork.Repository<Alert>().GetAllAsync();
        alerts.First().Status.Should().Be(AlertStatus.Closed);
        alerts.First().ResolvedAt.Should().NotBeNull();
        alerts.First().ResolvedAt.Should().BeAfter(alert.GeneratedAt);
    }

    [Fact]
    public async Task ResolveAsync_OpenAlert_ShouldTransitionToClosed()
    {
        // Arrange
        var alert = new Alert
        {
            Id = 1,
            ApiEndpointId = 5,
            Severity = AlertSeverity.Warning,
            Status = AlertStatus.Open,
            GeneratedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _unitOfWork.Repository<Alert>().Add(alert);

        var endpoint = new ApiEndpoint { Id = 5, Name = "Test Endpoint" };
        _unitOfWork.Repository<ApiEndpoint>().Add(endpoint);

        var resolveDto = new ResolveAlertDto
        {
            ResolvedAt = DateTime.UtcNow
        };

        // Act
        var result = await _sut.ResolveAsync(1, resolveDto, CancellationToken.None);

        // Assert
        result.Status.Should().Be(AlertStatus.Closed);
        result.ResolvedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var dbAlert = await _unitOfWork.Repository<Alert>().GetByIdAsync(1);
        dbAlert!.Status.Should().Be(AlertStatus.Closed);
    }

    [Fact]
    public async Task ResolveAsync_AlreadyClosedAlert_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var alert = new Alert
        {
            Id = 1,
            ApiEndpointId = 5,
            Status = AlertStatus.Closed,
            GeneratedAt = DateTime.UtcNow.AddMinutes(-5),
            ResolvedAt = DateTime.UtcNow.AddMinutes(-2)
        };
        _unitOfWork.Repository<Alert>().Add(alert);

        var resolveDto = new ResolveAlertDto { ResolvedAt = DateTime.UtcNow };

        // Act
        Func<Task> act = async () => await _sut.ResolveAsync(1, resolveDto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Alert is already closed.");
    }

    [Fact]
    public async Task ResolveAsync_NonExistentAlert_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var resolveDto = new ResolveAlertDto { ResolvedAt = DateTime.UtcNow };

        // Act
        Func<Task> act = async () => await _sut.ResolveAsync(999, resolveDto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Alert with ID 999 was not found.");
    }
}
