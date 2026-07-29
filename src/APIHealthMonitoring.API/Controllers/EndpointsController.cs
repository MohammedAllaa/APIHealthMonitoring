using APIHealthMonitoring.Application.DTOs.Endpoints;
using APIHealthMonitoring.Application.Interfaces.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIHealthMonitoring.API.Controllers;

/// <summary>
/// Provides CRUD and lifecycle endpoints for registered API endpoint monitoring targets.
/// </summary>
[ApiController]
[Route("api/endpoints")]
[Authorize]
[Produces("application/json")]
public class EndpointsController : ControllerBase
{
    private readonly IApiEndpointService _service;

    public EndpointsController(IApiEndpointService service)
    {
        _service = service;
    }

    // -------------------------------------------------------------------------
    // POST /api/endpoints   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Register a new API endpoint for monitoring.</summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiEndpointResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateApiEndpointDto request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // GET /api/endpoints   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns a paginated, filtered list of registered endpoints.</summary>
    [HttpGet]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(Application.Specifications.PaginatedResult<ApiEndpointSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] ApiEndpointPagedRequestDto request,
        CancellationToken ct)
    {
        var result = await _service.GetPagedAsync(request, ct);
        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /api/endpoints/{id}   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns full details for a single registered endpoint.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(ApiEndpointResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /api/endpoints/{id}   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Updates an existing endpoint registration. All fields are optional.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiEndpointResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateApiEndpointDto request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.UpdateAsync(id, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // DELETE /api/endpoints/{id}   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Permanently removes an endpoint from the registry.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /api/endpoints/{id}/activate   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Activates monitoring for the specified endpoint.</summary>
    [HttpPut("{id:int}/activate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Activate([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /api/endpoints/{id}/deactivate   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Deactivates monitoring for the specified endpoint.</summary>
    [HttpPut("{id:int}/deactivate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            await _service.DeactivateAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
