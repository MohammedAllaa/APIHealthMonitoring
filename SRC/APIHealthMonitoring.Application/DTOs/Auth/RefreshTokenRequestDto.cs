using System.ComponentModel.DataAnnotations;

namespace APIHealthMonitoring.Application.DTOs.Auth;

/// <summary>
/// Payload sent to exchange an expired access token for a new token pair.
/// </summary>
public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
