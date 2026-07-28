using APIHealthMonitoring.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APIHealthMonitoring.Persistence.Data;

/// <summary>
/// Seeds the database with required roles and a default Administrator account
/// on first startup. Idempotent — safe to call on every application start.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds roles and the default admin user.
    /// Call this from Program.cs after <c>app.Build()</c>.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger      = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationUser>>();

        // -------------------------------------------------------------------------
        // 1. Seed Roles
        // -------------------------------------------------------------------------

        var roles = new[]
        {
            new ApplicationRole { Name = "Administrator", Description = "Full access — register/modify/delete APIs, manage config, view all." },
            new ApplicationRole { Name = "Viewer",        Description = "Read-only — dashboard, reports, search." },
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                    logger.LogInformation("Role '{Role}' created.", role.Name);
                else
                    logger.LogError("Failed to create role '{Role}': {Errors}",
                        role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // -------------------------------------------------------------------------
        // 2. Seed Default Administrator
        // -------------------------------------------------------------------------

        const string adminEmail    = "admin@apihealthmonitoring.com";
        const string adminPassword = "Admin@12345";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            var admin = new ApplicationUser
            {
                UserName  = adminEmail,
                Email     = adminEmail,
                FirstName = "System",
                LastName  = "Administrator",
                CreatedAt = DateTime.UtcNow,
                IsActive  = true,
                EmailConfirmed = true,
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Administrator");
                logger.LogInformation(
                    "Default admin seeded. Email: {Email} | Password: {Password}",
                    adminEmail, adminPassword);
            }
            else
            {
                logger.LogError("Failed to seed admin: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
