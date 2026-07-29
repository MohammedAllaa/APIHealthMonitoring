using APIHealthMonitoring.Application.Interfaces.HealthChecks;
using APIHealthMonitoring.Application.Specifications.Endpoints;
using APIHealthMonitoring.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace APIHealthMonitoring.Infrastructure.HealthChecks.BackgroundServices;

/// <summary>
/// Singleton hosted background service that continuously polls all active API endpoints
/// according to their configured intervals. Each endpoint runs independently so one
/// failure never delays others. Uses IServiceScopeFactory to safely resolve scoped
/// DbContext services inside this singleton host.
/// </summary>
public class MonitoringBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonitoringBackgroundService> _logger;

    // Maximum simultaneous HTTP checks across all APIs
    private static readonly SemaphoreSlim _semaphore = new(100, 100);

    // Per-endpoint last-run tracker: endpointId → last execution time
    private readonly Dictionary<int, DateTime> _lastRun = new();

    public MonitoringBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<MonitoringBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonitoringBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCheckCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in monitoring loop.");
            }

            // Polling tick: every 5 seconds so we don't spin at 100% CPU
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("MonitoringBackgroundService stopped.");
    }

    private async Task RunCheckCycleAsync(CancellationToken ct)
    {
        List<ApiEndpoint> activeEndpoints;

        // Resolve scoped services for data access
        using (var scope = _scopeFactory.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<Application.Interfaces.IUnitOfWork>();

            var spec = new ActiveApiEndpointsSpec();
            activeEndpoints = (await unitOfWork.Repository<ApiEndpoint>()
                .GetAllWithSpecAsync(spec, ct)).ToList();
        }

        if (!activeEndpoints.Any()) return;

        var tasks = activeEndpoints
            .Where(e => ShouldCheck(e))
            .Select(e => RunSingleCheckAsync(e, ct))
            .ToList();

        if (tasks.Any())
            await Task.WhenAll(tasks);
    }

    private bool ShouldCheck(ApiEndpoint endpoint)
    {
        if (!_lastRun.TryGetValue(endpoint.Id, out var last))
            return true;

        return (DateTime.UtcNow - last).TotalSeconds >= endpoint.IntervalSeconds;
    }

    private async Task RunSingleCheckAsync(ApiEndpoint endpoint, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            _lastRun[endpoint.Id] = DateTime.UtcNow;

            using var scope      = _scopeFactory.CreateScope();
            var executor         = scope.ServiceProvider.GetRequiredService<IHealthCheckExecutor>();
            var healthCheckSvc   = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();

            var result = await executor.ExecuteAsync(endpoint, ct);
            await healthCheckSvc.SaveResultAsync(result, ct);

            _logger.LogDebug(
                "Check complete for '{Name}': {Status} in {Ms}ms",
                endpoint.Name,
                result.IsSuccessful ? "OK" : "FAIL",
                result.ResponseTimeMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing check for endpoint '{Name}'", endpoint.Name);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
