namespace APIHealthMonitoring.Application.DTOs.Auth;

/// <summary>
/// Returned after a successful login or token refresh.
/// Contains everything the client needs to authenticate subsequent requests.
/// </summary>
public class AuthResponseDto
{
    /// <summary>Short-lived JWT to be sent as Bearer token on every request.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Long-lived opaque token used exclusively to obtain a new AccessToken.</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>UTC expiry of the AccessToken.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>The authenticated user's GUID.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>The authenticated user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>List of roles assigned to the user (e.g. ["Administrator"]).</summary>
    public IList<string> Roles { get; set; } = new List<string>();
}
