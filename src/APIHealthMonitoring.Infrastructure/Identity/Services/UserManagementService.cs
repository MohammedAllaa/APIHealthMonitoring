using APIHealthMonitoring.Application.DTOs.Users;
using APIHealthMonitoring.Application.Interfaces.Auth;
using APIHealthMonitoring.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APIHealthMonitoring.Infrastructure.Identity.Services;

/// <summary>
/// Implements administrative user management:
/// listing users and toggling their active state.
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <inheritdoc />
    public async Task<IList<UserProfileDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        var profiles = new List<UserProfileDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            profiles.Add(MapToDto(user, roles));
        }

        return profiles;
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    /// <inheritdoc />
    public async Task ActivateUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"User '{id}' not found.");

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);
        EnsureSuccess(result);
    }

    /// <inheritdoc />
    public async Task DeactivateUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"User '{id}' not found.");

        user.IsActive = false;

        // Also invalidate any active refresh token so the session ends immediately
        user.RefreshToken          = null;
        user.RefreshTokenExpiresAt = null;

        var result = await _userManager.UpdateAsync(user);
        EnsureSuccess(result);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static UserProfileDto MapToDto(ApplicationUser user, IList<string> roles)
        => new UserProfileDto
        {
            Id        = user.Id,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Email     = user.Email ?? string.Empty,
            Roles     = roles,
            CreatedAt = user.CreatedAt,
            IsActive  = user.IsActive,
        };

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }
    }
}
