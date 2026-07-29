using APIHealthMonitoring.Application.Constants;
using APIHealthMonitoring.Application.DTOs.Alerts;
using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Alerts;
using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Application.Specifications.Alerts;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace APIHealthMonitoring.Infrastructure.Alerts.Services;

/// <summary>
/// Handles alert lifecycle: creation (with deduplication), resolution, and queries.
/// </summary>
public class AlertService : IAlertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<AlertService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache      = cache;
        _logger     = logger;
    }

    // -------------------------------------------------------------------------
    // Create (dedup-aware)
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task CreateIfNotDuplicateAsync(
        int apiEndpointId,
        AlertSeverity severity,
        string message,
        CancellationToken ct = default)
    {
        // Deduplication: skip if an open alert with same API + severity already exists
        var dupSpec    = new OpenAlertsByApiSpec(apiEndpointId, severity);
        var existingAlert = await _unitOfWork.Repository<Alert>().GetEntityWithSpecAsync(dupSpec, ct);
        if (existingAlert is not null)
        {
            _logger.LogInformation(
                "Duplicate open alert ignored for ApiEndpointId {ApiEndpointId} with Severity {Severity}",
                apiEndpointId, severity);
            return;
        }

        var alert = new Alert
        {
            ApiEndpointId = apiEndpointId,
            Severity      = severity,
            Message       = message,
            GeneratedAt   = DateTime.UtcNow,
            Status        = AlertStatus.Open,
        };

        _unitOfWork.Repository<Alert>().Add(alert);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Alert created for ApiEndpointId {ApiEndpointId}: {Message} (Severity: {Severity})",
            apiEndpointId, message, severity);

        // Module 10 — alert count changed; invalidate summary
        _cache.Remove(CacheKeys.DashboardSummary);
    }

    // -------------------------------------------------------------------------
    // Auto-Resolution (called when API recovers to Healthy)
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task AutoResolveForEndpointAsync(int apiEndpointId, CancellationToken ct = default)
    {
        var spec        = new OpenAlertsByEndpointSpec(apiEndpointId);
        var openAlerts  = await _unitOfWork.Repository<Alert>().GetAllWithSpecAsync(spec, ct);

        if (!openAlerts.Any()) return;

        var now = DateTime.UtcNow;
        foreach (var alert in openAlerts)
        {
            alert.Status     = AlertStatus.Closed;
            alert.ResolvedAt = now;
            _unitOfWork.Repository<Alert>().Update(alert);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Auto-resolved {AlertCount} open alert(s) for ApiEndpointId {ApiEndpointId}",
            openAlerts.Count, apiEndpointId);

        // Module 10 — alert count changed; invalidate summary
        _cache.Remove(CacheKeys.DashboardSummary);
    }

    // -------------------------------------------------------------------------
    // Manual Resolution (by Admin)
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<AlertResponseDto> ResolveAsync(
        int alertId, ResolveAlertDto dto, CancellationToken ct = default)
    {
        var alert = await _unitOfWork.Repository<Alert>().GetByIdAsync(alertId, ct)
            ?? throw new KeyNotFoundException($"Alert with ID {alertId} was not found.");

        if (alert.Status == AlertStatus.Closed)
            throw new InvalidOperationException("Alert is already closed.");

        var resolvedAt = dto.ResolvedAt?.ToUniversalTime() ?? DateTime.UtcNow;

        if (resolvedAt < alert.GeneratedAt)
            throw new InvalidOperationException("ResolvedAt must be after GeneratedAt.");

        alert.Status     = AlertStatus.Closed;
        alert.ResolvedAt = resolvedAt;

        _unitOfWork.Repository<Alert>().Update(alert);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("AlertId {AlertId} manually resolved", alertId);

        // Module 10 — alert resolved; invalidate summary
        _cache.Remove(CacheKeys.DashboardSummary);

        // Load endpoint name for the response
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(alert.ApiEndpointId, ct);
        alert.ApiEndpoint = endpoint;

        return MapToDto(alert);
    }

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<PaginatedResult<AlertResponseDto>> GetPagedAsync(
        AlertPagedRequestDto request, CancellationToken ct = default)
    {
        if (request.FromDate.HasValue && request.ToDate.HasValue &&
            request.FromDate.Value > request.ToDate.Value)
        {
            throw new InvalidOperationException("FromDate cannot be after ToDate.");
        }

        var dataSpec  = new AlertSearchSpec(request);
        var countSpec = new AlertSearchCountSpec(request);

        var alerts = await _unitOfWork.Repository<Alert>().GetAllWithSpecAsync(dataSpec, ct);
        var total  = await _unitOfWork.Repository<Alert>().CountAsync(countSpec, ct);

        // Batch-load endpoint names
        var endpointIds = alerts.Select(a => a.ApiEndpointId).Distinct().ToList();
        var endpoints   = (await _unitOfWork.Repository<ApiEndpoint>().GetAllAsync(ct))
                           .Where(e => endpointIds.Contains(e.Id))
                           .ToDictionary(e => e.Id, e => e.Name);

        var dtos = alerts.Select(a => MapToDto(a, endpoints.TryGetValue(a.ApiEndpointId, out var n) ? n : null)).ToList();

        return new PaginatedResult<AlertResponseDto>(
            dtos, total, request.PageIndex, Math.Min(request.PageSize, 100));
    }

    /// <inheritdoc />
    public async Task<AlertResponseDto> GetByIdAsync(int alertId, CancellationToken ct = default)
    {
        var alert = await _unitOfWork.Repository<Alert>().GetByIdAsync(alertId, ct)
            ?? throw new KeyNotFoundException($"Alert with ID {alertId} was not found.");

        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(alert.ApiEndpointId, ct);
        alert.ApiEndpoint = endpoint;

        return MapToDto(alert);
    }

    // -------------------------------------------------------------------------
    // Mapping
    // -------------------------------------------------------------------------

    private static AlertResponseDto MapToDto(Alert a, string? apiName = null) => new()
    {
        AlertId       = a.Id,
        ApiEndpointId = a.ApiEndpointId,
        ApiName       = a.ApiEndpoint?.Name ?? apiName ?? string.Empty,
        Severity      = a.Severity,
        Message       = a.Message,
        GeneratedAt   = a.GeneratedAt,
        ResolvedAt    = a.ResolvedAt,
        Status        = a.Status,
    };
}
