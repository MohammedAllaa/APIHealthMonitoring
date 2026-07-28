using System.Text;
using APIHealthMonitoring.Application.Interfaces.Auth;
using APIHealthMonitoring.Application.Interfaces.Endpoints;
using APIHealthMonitoring.Application.Interfaces.HealthChecks;
using APIHealthMonitoring.Infrastructure.Endpoints.Services;
using APIHealthMonitoring.Infrastructure.HealthChecks.Services;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Infrastructure.Identity.Services;
using APIHealthMonitoring.Infrastructure.Identity.Settings;
using APIHealthMonitoring.Persistence.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace APIHealthMonitoring.Infrastructure;

/// <summary>
/// DI extension method for the Infrastructure layer.
/// Registers Identity, JWT authentication, and all auth services.
/// Called once from Program.cs alongside AddPersistenceServices.
/// </summary>
public static class IdentityServiceRegistration
{
    /// <summary>
    /// Registers all Infrastructure / Security services with the DI container.
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <param name="configuration">
    /// Application configuration — reads <c>JwtSettings</c> section.
    /// </param>
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        // -------------------------------------------------------------------------
        // JWT Settings — bind and register as IOptions<JwtSettings>
        // -------------------------------------------------------------------------

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        // -------------------------------------------------------------------------
        // ASP.NET Core Identity
        // -------------------------------------------------------------------------

        // Identity is configured here but the DbContext it uses
        // (AppDbContext : IdentityDbContext) is registered in Persistence.
        // AddIdentityCore does not add external login providers or default UI.
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Password policy — enforced at registration and change-password
                options.Password.RequireDigit           = true;
                options.Password.RequireLowercase       = true;
                options.Password.RequireUppercase        = true;
                options.Password.RequireNonAlphanumeric  = true;
                options.Password.RequiredLength          = 8;

                // Lockout — disable for now; can be enabled per environment
                options.Lockout.AllowedForNewUsers = false;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            // EF stores — wire Identity to AppDbContext (IdentityDbContext).
            // Infrastructure → Persistence reference is intentional at this layer.
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // -------------------------------------------------------------------------
        // JWT Bearer Authentication
        // -------------------------------------------------------------------------

        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "JwtSettings section is missing from configuration.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                    ValidateIssuer   = true,
                    ValidIssuer      = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience    = jwtSettings.Audience,

                    ValidateLifetime        = true,
                    ClockSkew               = TimeSpan.Zero, // no grace period
                    RoleClaimType           = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType           = System.Security.Claims.ClaimTypes.NameIdentifier,
                };
            });

        // -------------------------------------------------------------------------
        // Application Services
        // -------------------------------------------------------------------------

        services.AddScoped<ITokenService,          TokenService>();
        services.AddScoped<IAuthService,           AuthService>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        // Module 2 — API Endpoint Registry
        services.AddScoped<IApiEndpointService, ApiEndpointService>();

        // Module 3 — Monitoring Configuration
        services.AddScoped<IMonitoringConfigService, MonitoringConfigService>();

        // Module 4 — Health Check Engine
        services.AddScoped<IHealthCheckExecutor,   HealthCheckExecutor>();
        services.AddScoped<IHealthStatusEvaluator, HealthStatusEvaluator>();
        services.AddScoped<IHealthCheckService,    HealthCheckService>();

        return services;
    }
}
