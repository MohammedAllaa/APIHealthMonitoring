namespace APIHealthMonitoring.Infrastructure.Identity.Settings;

/// <summary>
/// Strongly-typed settings bound from the <c>JwtSettings</c> section
/// of <c>appsettings.json</c>.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>Signing secret — must be ≥ 256 bits (32 characters) for HMAC-SHA256.</summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>Token issuer claim value.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Token audience claim value.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>How long (in minutes) the access token is valid.</summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>How long (in days) the refresh token is valid.</summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
