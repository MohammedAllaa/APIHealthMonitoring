using System.ComponentModel.DataAnnotations;

namespace APIHealthMonitoring.Application.DTOs.Auth;

/// <summary>
/// Payload sent by a user to authenticate and receive tokens.
/// </summary>
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
