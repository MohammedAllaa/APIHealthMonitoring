using APIHealthMonitoring.Application.DTOs.Users;

namespace APIHealthMonitoring.Application.Interfaces.Auth;

/// <summary>
/// Defines administrative user management operations:
/// listing users and toggling their active state.
/// </summary>
public interface IUserManagementService
{
    /// <summary>Returns a paginated list of all registered users.</summary>
    Task<IList<UserProfileDto>> GetAllUsersAsync();

    /// <summary>Returns a single user by their GUID, or null if not found.</summary>
    Task<UserProfileDto?> GetUserByIdAsync(string id);

    /// <summary>Sets the user's <c>IsActive</c> flag to <c>true</c>.</summary>
    Task ActivateUserAsync(string id);

    /// <summary>Sets the user's <c>IsActive</c> flag to <c>false</c>.</summary>
    Task DeactivateUserAsync(string id);
}
