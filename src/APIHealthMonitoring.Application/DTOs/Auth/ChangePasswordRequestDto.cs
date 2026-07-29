using System.ComponentModel.DataAnnotations;

namespace APIHealthMonitoring.Application.DTOs.Auth;

/// <summary>
/// Payload sent by an authenticated user to change their own password.
/// </summary>
public class ChangePasswordRequestDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
