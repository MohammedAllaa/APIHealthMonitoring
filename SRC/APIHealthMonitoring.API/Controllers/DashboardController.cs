using APIHealthMonitoring.Application.DTOs.Dashboard;
using APIHealthMonitoring.Application.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIHealthMonitoring.API.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Returns overall platform operational metrics summary.</summary>
    [HttpGet("api/dashboard/summary")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var summary = await _dashboardService.GetSummaryAsync(ct);
        return Ok(summary);
    }

    /// <summary>Returns paged dashboard cards for monitored API endpoints.</summary>
    [HttpGet("api/dashboard/apis")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(Application.Specifications.PaginatedResult<ApiDashboardCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApiCards(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var cards = await _dashboardService.GetApiCardsAsync(pageIndex, pageSize, ct);
        return Ok(cards);
    }

    /// <summary>Returns full historical performance statistics for a specific API endpoint.</summary>
    [HttpGet("api/endpoints/{id:int}/stats")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(ApiHistoricalStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEndpointStats([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            var stats = await _dashboardService.GetEndpointStatsAsync(id, ct);
            return Ok(stats);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
