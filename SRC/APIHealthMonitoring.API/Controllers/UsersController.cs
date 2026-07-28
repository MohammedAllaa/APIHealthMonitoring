using APIHealthMonitoring.Application.DTOs.Users;
using APIHealthMonitoring.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIHealthMonitoring.API.Controllers;

/// <summary>
/// Provides administrative user management endpoints:
/// list users, get by ID, activate, and deactivate.
/// All endpoints require the <c>Administrator</c> role.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    // -------------------------------------------------------------------------
    // GET /api/users   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Returns a list of all registered users.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IList<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManagementService.GetAllUsersAsync();
        return Ok(users);
    }

    // -------------------------------------------------------------------------
    // GET /api/users/{id}   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Returns a single user by their GUID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var user = await _userManagementService.GetUserByIdAsync(id);
        if (user is null)
            return NotFound(new { message = $"User '{id}' not found." });

        return Ok(user);
    }

    // -------------------------------------------------------------------------
    // PUT /api/users/{id}/activate   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Activates a user account so they may log in.</summary>
    [HttpPut("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Activate([FromRoute] string id)
    {
        try
        {
            await _userManagementService.ActivateUserAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /api/users/{id}/deactivate   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>
    /// Deactivates a user account, preventing further logins
    /// and immediately invalidating any active refresh token.
    /// </summary>
    [HttpPut("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate([FromRoute] string id)
    {
        try
        {
            await _userManagementService.DeactivateUserAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
