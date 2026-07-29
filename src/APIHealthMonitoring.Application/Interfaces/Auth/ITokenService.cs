using System.Security.Claims;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Interfaces.Auth;

/// <summary>
/// Handles all JWT and refresh-token cryptographic operations.
/// Decoupled from identity management so it can be tested in isolation.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT access token containing the user's claims.
    /// </summary>
    /// <param name="user">The user for whom the token is generated.</param>
    /// <param name="roles">The roles to embed as claims.</param>
    /// <returns>A signed JWT string and its UTC expiry.</returns>
    (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IList<string> roles);

    /// <summary>
    /// Generates a cryptographically random opaque refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Decodes an expired (or valid) JWT and returns its <see cref="ClaimsPrincipal"/>
    /// without validating the lifetime — used only during refresh-token rotation.
    /// </summary>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
