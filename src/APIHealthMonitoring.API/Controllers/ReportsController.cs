using APIHealthMonitoring.Application.DTOs.Reports;
using APIHealthMonitoring.Application.Interfaces.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIHealthMonitoring.API.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    /// <summary>Generates a daily health report for a given date and optional API filter.</summary>
    [HttpGet("api/reports/daily")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(List<DailyHealthReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDailyReport(
        [FromQuery] DateTime? date,
        [FromQuery] int? apiId,
        CancellationToken ct)
    {
        var targetDate = date?.ToUniversalTime() ?? DateTime.UtcNow.Date;
        var report = await _reportingService.GetDailyReportAsync(targetDate, apiId, ct);
        return Ok(report);
    }

    /// <summary>Generates a weekly trend report starting from a specified Monday date.</summary>
    [HttpGet("api/reports/weekly")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(List<WeeklyTrendReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWeeklyReport(
        [FromQuery] DateTime? weekStart,
        CancellationToken ct)
    {
        var targetWeekStart = weekStart?.ToUniversalTime() ?? DateTime.UtcNow.Date;
        var report = await _reportingService.GetWeeklyTrendReportAsync(targetWeekStart, ct);
        return Ok(report);
    }

    /// <summary>Generates a monthly performance report ranking top/bottom APIs.</summary>
    [HttpGet("api/reports/monthly")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(MonthlyPerformanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        try
        {
            var report = await _reportingService.GetMonthlyPerformanceReportAsync(year, month, ct);
            return Ok(report);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
