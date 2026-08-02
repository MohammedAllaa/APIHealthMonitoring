using APIHealthMonitoring.Application.DTOs.Auth;
using APIHealthMonitoring.Application.Interfaces.Auth;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Infrastructure.Identity.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace APIHealthMonitoring.Infrastructure.Identity.Services;

/// <summary>
/// Implements all authentication workflows:
/// register, login, refresh, logout, and password change.
/// </summary>
public class AuthService : IAuthService
{
    private static readonly string[] AllowedRoles = { "Administrator", "Viewer" };

    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly ITokenService                 _tokenService;
    private readonly JwtSettings                   _jwtSettings;
    private readonly ILogger<AuthService>          _logger;

    public AuthService(
        UserManager<ApplicationUser>  userManager,
        ITokenService                 tokenService,
        IOptions<JwtSettings>         jwtSettings,
        ILogger<AuthService>          logger)
    {
        _userManager  = userManager;
        _tokenService = tokenService;
        _jwtSettings  = jwtSettings.Value;
        _logger       = logger;
    }

    // -------------------------------------------------------------------------
    // Register
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // --- Validate role ---
        if (!AllowedRoles.Contains(request.Role))
            throw new InvalidOperationException(
                $"Role must be one of: {string.Join(", ", AllowedRoles)}.");

        // --- Ensure email uniqueness ---
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName  = request.Email,
            Email     = request.Email,
            FirstName = request.FirstName,
            LastName  = request.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive  = true,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for Email {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Role assignment failed for UserId {UserId}: {Errors}", user.Id, errors);
            throw new InvalidOperationException($"Role assignment failed: {errors}");
        }

        _logger.LogInformation("User registered successfully with UserId {UserId} and Role {Role}", user.Id, request.Role);

        var roles = await _userManager.GetRolesAsync(user);
        return BuildAuthResponse(user, roles);
    }

    // -------------------------------------------------------------------------
    // Login
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Login attempt failed for non-existent Email {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt failed for inactive UserId {UserId}", user.Id);
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Invalid password provided for UserId {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        _logger.LogInformation("User logged in successfully with UserId {UserId}", user.Id);

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAndPersistTokensAsync(user, roles);
    }

    // -------------------------------------------------------------------------
    // Refresh Token
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        // We need the user Id from the *expired* access token — but the client
        // only sends the refresh token here. We look up the user by matching
        // the stored refresh token directly.
        var user = _userManager.Users
            .FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

        if (user is null)
        {
            _logger.LogWarning("Token refresh attempt failed with invalid refresh token.");
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Token refresh attempt failed for inactive UserId {UserId}", user.Id);
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        if (user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Token refresh attempt failed for expired token on UserId {UserId}", user.Id);
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }

        _logger.LogInformation("Token refreshed successfully for UserId {UserId}", user.Id);

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAndPersistTokensAsync(user, roles);
    }

    // -------------------------------------------------------------------------
    // Logout
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.RefreshToken          = null;
        user.RefreshTokenExpiresAt = null;

        await _userManager.UpdateAsync(user);
        _logger.LogInformation("User logged out successfully with UserId {UserId}", userId);
    }

    // -------------------------------------------------------------------------
    // Change Password
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password change failed for UserId {UserId}: {Errors}", userId, errors);
            throw new InvalidOperationException($"Password change failed: {errors}");
        }

        // Invalidate refresh tokens after a password change for security
        user.RefreshToken          = null;
        user.RefreshTokenExpiresAt = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Password changed successfully for UserId {UserId}", userId);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private AuthResponseDto BuildAuthResponse(ApplicationUser user, IList<string> roles)
    {
        var (token, expiresAt) = _tokenService.GenerateAccessToken(user, roles);
        return new AuthResponseDto
        {
            AccessToken  = token,
            RefreshToken = user.RefreshToken ?? string.Empty,
            ExpiresAt    = expiresAt,
            UserId       = user.Id,
            Email        = user.Email ?? string.Empty,
            Roles        = roles,
        };
    }

    private async Task<AuthResponseDto> BuildAndPersistTokensAsync(
        ApplicationUser user, IList<string> roles)
    {
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken             = _tokenService.GenerateRefreshToken();

        user.RefreshToken          = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt    = expiresAt,
            UserId       = user.Id,
            Email        = user.Email ?? string.Empty,
            Roles        = roles,
        };
    }
}
