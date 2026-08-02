using APIHealthMonitoring.Application.DTOs.Auth;

namespace APIHealthMonitoring.Application.Interfaces.Auth;

/// <summary>
/// Defines authentication operations: register, login, token refresh, logout,
/// and password change.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Creates a new user account and assigns the specified role.
    /// Can only be invoked by an Administrator (enforced at the controller level).
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Validates credentials, checks the account is active,
    /// and returns a fresh access + refresh token pair.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Validates the supplied refresh token, issues a new access token,
    /// and rotates the refresh token.
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

    /// <summary>
    /// Clears the stored refresh token so the user cannot silently re-authenticate.
    /// </summary>
    Task LogoutAsync(string userId);

    /// <summary>
    /// Changes the authenticated user's own password after verifying the current one.
    /// </summary>
    Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
}
