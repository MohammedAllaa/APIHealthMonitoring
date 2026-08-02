using Microsoft.AspNetCore.Identity;

namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Extends ASP.NET Core Identity's <see cref="IdentityRole"/> with
/// a human-readable description of the role's purpose.
/// </summary>
public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// A human-readable explanation of what this role permits.
    /// Example: "Full access — register/modify/delete APIs, manage config, view all."
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
