using System.ComponentModel.DataAnnotations;

namespace APIHealthMonitoring.Application.DTOs.Auth;

/// <summary>
/// Payload sent by an Administrator to register a new user account.
/// </summary>
public class RegisterRequestDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Must be ≥ 8 chars, contain one uppercase, one digit, one special character.
    /// Enforced by ASP.NET Core Identity's PasswordValidator.
    /// </summary>
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Must be "Administrator" or "Viewer".</summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}
