using Microsoft.AspNetCore.Identity;

namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Extends ASP.NET Core Identity's <see cref="IdentityUser"/> with
/// application-specific profile and refresh-token fields.
/// The string type parameter means the primary key is a GUID stored as varchar.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // -------------------------------------------------------------------------
    // Profile
    // -------------------------------------------------------------------------

    /// <summary>The user's given (first) name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>The user's family (last) name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>UTC timestamp of account creation.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Controls whether the user may authenticate.
    /// Inactive users are rejected at login time regardless of credentials.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // -------------------------------------------------------------------------
    // Refresh Token
    // -------------------------------------------------------------------------

    /// <summary>
    /// The opaque refresh token currently associated with this user.
    /// Null when the user has never logged in or has been logged out.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// UTC expiry of the stored refresh token.
    /// Null when no refresh token is active.
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }
}
