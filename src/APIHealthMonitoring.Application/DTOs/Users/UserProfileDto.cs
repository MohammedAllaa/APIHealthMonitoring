namespace APIHealthMonitoring.Application.DTOs.Users;

/// <summary>
/// Public-facing representation of a user account.
/// Returned by user management endpoints.
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
