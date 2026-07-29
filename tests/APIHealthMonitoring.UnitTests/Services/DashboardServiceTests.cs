using APIHealthMonitoring.Application.Constants;
using APIHealthMonitoring.Application.DTOs.Dashboard;
using APIHealthMonitoring.Application.Interfaces.Reporting;
using APIHealthMonitoring.Application.Settings;
using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;
using APIHealthMonitoring.Infrastructure.Dashboard.Services;
using APIHealthMonitoring.UnitTests.Common.Builders;
using APIHealthMonitoring.UnitTests.Common.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace APIHealthMonitoring.UnitTests.Services;

public class DashboardServiceTests
{
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly Mock<IAvailabilityCalculator> _calculatorMock;
    private readonly FakeCacheService _cacheService;
    private readonly Mock<IOptions<CacheSettings>> _cacheSettingsMock;
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _unitOfWork = new FakeUnitOfWork();
        _calculatorMock = new Mock<IAvailabilityCalculator>();
        _cacheService = new FakeCacheService();
        _cacheSettingsMock = new Mock<IOptions<CacheSettings>>();

        _cacheSettingsMock.Setup(s => s.Value).Returns(new CacheSettings
        {
            DashboardSummaryExpirationSeconds = 30,
            DashboardApiCardsExpirationSeconds = 30,
            ApiStatsExpirationSeconds = 60
        });

        _sut = new DashboardService(
            _unitOfWork,
            _calculatorMock.Object,
            _cacheService,
            _cacheSettingsMock.Object
        );
    }

    [Fact]
    public async Task GetSummaryAsync_CacheHit_ShouldReturnCachedDataWithoutQueryingDb()
    {
        // Arrange
        var cachedSummary = new DashboardSummaryDto
        {
            TotalApis = 5,
            HealthyCount = 4,
            WarningCount = 1,
            OverallAvailability = 98.5m
        };

        _cacheService.Set(CacheKeys.DashboardSummary, cachedSummary, TimeSpan.FromMinutes(1));

        // Act
        var result = await _sut.GetSummaryAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedSummary);
        
        // Ensure no DB queries were made (database repositories should be empty)
        var endpoints = await _unitOfWork.Repository<ApiEndpoint>().GetAllAsync();
        endpoints.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSummaryAsync_CacheMiss_ShouldQueryDbCalculateStatsAndCacheResult()
    {
        // Arrange
        var endpoint1 = new ApiEndpointBuilder().WithId(1).WithIsActive(true).WithCurrentStatus(ApiHealthStatus.Healthy).Build();
        var endpoint2 = new ApiEndpointBuilder().WithId(2).WithIsActive(true).WithCurrentStatus(ApiHealthStatus.Warning).Build();
        
        _unitOfWork.Repository<ApiEndpoint>().Add(endpoint1);
        _unitOfWork.Repository<ApiEndpoint>().Add(endpoint2);

        // Mock calculator
        _calculatorMock.Setup(c => c.Calculate(It.IsAny<IEnumerable<HealthCheck>>()))
            .Returns((95.0m, 120.0, 10, 9, 1));

        // Ensure cache is empty
        _cacheService.Remove(CacheKeys.DashboardSummary);

        // Act
        var result = await _sut.GetSummaryAsync(CancellationToken.None);

        // Assert
        result.TotalApis.Should().Be(2);
        result.HealthyCount.Should().Be(1);
        result.WarningCount.Should().Be(1);
        result.OverallAvailability.Should().Be(95.0m);
        result.AvgResponseTimeMs.Should().Be(120.0);

        // Verify it was stored in the cache
        _cacheService.TryGetValue<DashboardSummaryDto>(CacheKeys.DashboardSummary, out var cached).Should().BeTrue();
        cached.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task GetApiCardsAsync_CacheHit_ShouldReturnCachedCards()
    {
        // Arrange
        var cachedCards = new PaginatedResult<ApiDashboardCardDto>(
            new List<ApiDashboardCardDto> { new ApiDashboardCardDto { Id = 1, Name = "Test" } },
            1, 1, 10
        );
        var cacheKey = $"{CacheKeys.DashboardApiCards}:1:10";
        _cacheService.Set(cacheKey, cachedCards, TimeSpan.FromMinutes(1));

        // Act
        var result = await _sut.GetApiCardsAsync(1, 10, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedCards);
    }

    [Fact]
    public async Task GetEndpointStatsAsync_CacheMiss_ShouldFetchFromDbAndCacheResult()
    {
        // Arrange
        var endpoint = new ApiEndpointBuilder().WithId(42).WithName("Demo Endpoint").Build();
        _unitOfWork.Repository<ApiEndpoint>().Add(endpoint);

        _calculatorMock.Setup(c => c.Calculate(It.IsAny<IEnumerable<HealthCheck>>()))
            .Returns((99.0m, 80.0, 100, 99, 1));

        var cacheKey = $"{CacheKeys.ApiStatsPrefix}42";
        _cacheService.Remove(cacheKey);

        // Act
        var result = await _sut.GetEndpointStatsAsync(42, CancellationToken.None);

        // Assert
        result.ApiEndpointId.Should().Be(42);
        result.ApiName.Should().Be("Demo Endpoint");
        result.MonthlyAvailability.Should().Be(99.0m);

        // Verify cache populated
        _cacheService.TryGetValue<ApiHistoricalStatsDto>(cacheKey, out var cached).Should().BeTrue();
        cached.Should().BeEquivalentTo(result);
    }
}
