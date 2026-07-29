using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace APIHealthMonitoring.Persistence.Data;

/// <summary>
/// Seeds the database with roles, sample users, endpoints, configurations,
/// 30 days of health check history, and sample alerts.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext   = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger      = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationUser>>();

        // Apply migrations automatically before seeding
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();

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
                await roleManager.CreateAsync(role);
                logger.LogInformation("Role '{Role}' created.", role.Name);
            }
        }

        // -------------------------------------------------------------------------
        // 2. Seed Users
        // -------------------------------------------------------------------------
        var usersToSeed = new[]
        {
            new { Email = "admin@healthmonitor.com",     Role = "Administrator", Pwd = "Admin@12345", First = "System", Last = "Admin" },
            new { Email = "viewer@healthmonitor.com",    Role = "Viewer",        Pwd = "Viewer@12345", First = "General", Last = "Viewer" },
            new { Email = "admin@apihealthmonitoring.com",Role = "Administrator", Pwd = "Admin@12345", First = "Legacy", Last = "Admin" }
        };

        foreach (var u in usersToSeed)
        {
            var existingUser = await userManager.FindByEmailAsync(u.Email);
            if (existingUser is null)
            {
                var user = new ApplicationUser
                {
                    UserName  = u.Email,
                    Email     = u.Email,
                    FirstName = u.First,
                    LastName  = u.Last,
                    CreatedAt = DateTime.UtcNow,
                    IsActive  = true,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, u.Pwd);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, u.Role);
                    logger.LogInformation("Seeded user: {Email} with role {Role}", u.Email, u.Role);
                }
                else
                {
                    logger.LogError("Failed to seed user {Email}: {Errors}", u.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // -------------------------------------------------------------------------
        // 3. Seed ApiEndpoints & Monitoring Configurations
        // -------------------------------------------------------------------------
        if (!dbContext.ApiEndpoints.Any())
        {
            logger.LogInformation("Seeding API Endpoints and configurations...");

            var sampleEndpoints = new[]
            {
                new { Name = "Payment Gateway API",        Env = Domain.Enums.Environment.Production,  Status = ApiHealthStatus.Healthy,  Url = "https://api.payments.com" },
                new { Name = "Customer Management API",    Env = Domain.Enums.Environment.Production,  Status = ApiHealthStatus.Healthy,  Url = "https://customers.domain.com" },
                new { Name = "Smart Metering API",         Env = Domain.Enums.Environment.Production,  Status = ApiHealthStatus.Warning,  Url = "https://smartmetering.internal" },
                new { Name = "Reporting Service",          Env = Domain.Enums.Environment.Production,  Status = ApiHealthStatus.Critical, Url = "https://reports.domain.com" },
                new { Name = "Mobile Auth API",            Env = Domain.Enums.Environment.Production,  Status = ApiHealthStatus.Healthy,  Url = "https://auth-mobile.domain.com" },
                new { Name = "Notification Service",       Env = Domain.Enums.Environment.UAT,         Status = ApiHealthStatus.Healthy,  Url = "https://uat-notifications.internal" },
                new { Name = "Billing API",                Env = Domain.Enums.Environment.UAT,         Status = ApiHealthStatus.Warning,  Url = "https://uat-billing.domain.com" },
                new { Name = "Asset Management API",       Env = Domain.Enums.Environment.QA,          Status = ApiHealthStatus.Healthy,  Url = "https://qa-assets.internal" },
                new { Name = "Inventory Service",          Env = Domain.Enums.Environment.QA,          Status = ApiHealthStatus.Healthy,  Url = "https://qa-inventory.internal" },
                new { Name = "User Directory API",         Env = Domain.Enums.Environment.QA,          Status = ApiHealthStatus.Critical, Url = "https://qa-users.internal" },
                new { Name = "Analytics API",              Env = Domain.Enums.Environment.Development,  Status = ApiHealthStatus.Healthy,  Url = "https://dev-analytics.local" },
                new { Name = "Export Service",             Env = Domain.Enums.Environment.Development,  Status = ApiHealthStatus.Healthy,  Url = "https://dev-export.local" },
                new { Name = "Webhook Relay",              Env = Domain.Enums.Environment.Development,  Status = ApiHealthStatus.Warning,  Url = "https://dev-webhooks.local" },
                new { Name = "SMS Gateway",                Env = Domain.Enums.Environment.Production,  Status = ApiHealthStatus.Healthy,  Url = "https://api.sms-gateway.com" },
                new { Name = "Email Service",              Env = Domain.Enums.Environment.Production,  Status = ApiHealthStatus.Healthy,  Url = "https://api.email-service.com" }
            };

            var now = DateTime.UtcNow;
            var rand = new Random(42); // deterministic seed

            foreach (var sample in sampleEndpoints)
            {
                var endpoint = new ApiEndpoint
                {
                    Name               = sample.Name,
                    BaseUrl            = sample.Url,
                    HealthEndpoint     = "/health",
                    HttpMethod         = Domain.Enums.HttpMethod.GET,
                    ExpectedStatusCode = 200,
                    TimeoutSeconds     = 5,
                    IntervalSeconds    = 60,
                    ServiceOwner       = "Operations Team",
                    Environment        = sample.Env,
                    IsActive           = true,
                    CurrentStatus      = sample.Status,
                    LastCheckedAt      = now,
                    ConsecutiveFailures = sample.Status == ApiHealthStatus.Critical ? 3 : 0
                };

                dbContext.ApiEndpoints.Add(endpoint);
                dbContext.SaveChanges(); // Save to get the ID

                var config = new MonitoringConfiguration
                {
                    ApiEndpointId         = endpoint.Id,
                    SlowThresholdMs       = 1000,
                    CriticalThresholdMs   = 2000,
                    FailureCountLimit     = 3,
                    AvailabilityThreshold = 99.0m
                };

                dbContext.MonitoringConfigurations.Add(config);
                dbContext.SaveChanges();

                // -------------------------------------------------------------------------
                // 4. Seed 30 Days of Health Check History per API
                // -------------------------------------------------------------------------
                logger.LogInformation("Generating 30 days of health checks for '{Name}'...", endpoint.Name);
                
                var startDate = now.AddDays(-30);
                var checkTime = startDate;

                var checksToInsert = new List<HealthCheck>();

                while (checkTime < now)
                {
                    var check = new HealthCheck
                    {
                        ApiEndpointId = endpoint.Id,
                        CheckedAt     = checkTime,
                        CreatedAt     = checkTime
                    };

                    // Determine execution result based on desired status
                    if (sample.Status == ApiHealthStatus.Healthy)
                    {
                        // 98% Success, fast response times (100 - 300ms)
                        bool isSuccess = rand.Next(100) < 98;
                        check.IsSuccessful = isSuccess;
                        check.ResponseTimeMs = rand.Next(100, 300);
                        check.StatusCode = isSuccess ? 200 : 500;
                        check.ErrorMessage = isSuccess ? null : "Internal Server Error";
                        check.ResponseSizeBytes = isSuccess ? rand.Next(500, 2000) : 0;
                    }
                    else if (sample.Status == ApiHealthStatus.Warning)
                    {
                        // Slower response times (800 - 1500ms), 92% success rate
                        bool isSlow = rand.Next(100) < 60;
                        bool isSuccess = rand.Next(100) < 92;
                        
                        check.IsSuccessful = isSuccess;
                        check.ResponseTimeMs = isSlow ? rand.Next(1000, 1600) : rand.Next(200, 500);
                        check.StatusCode = isSuccess ? 200 : 503;
                        check.ErrorMessage = isSuccess ? (isSlow ? "Slow Response" : null) : "Service Unavailable";
                        check.ResponseSizeBytes = isSuccess ? rand.Next(500, 2000) : 0;
                    }
                    else // Critical
                    {
                        // Very high response times, frequent failures (50% success rate)
                        bool isSuccess = rand.Next(100) < 50;
                        check.IsSuccessful = isSuccess;
                        check.ResponseTimeMs = rand.Next(1800, 2800);
                        check.StatusCode = isSuccess ? 200 : 500;
                        check.ErrorMessage = isSuccess ? "Degraded performance" : "Internal Server Error";
                        check.ResponseSizeBytes = isSuccess ? rand.Next(500, 2000) : 0;
                    }

                    checksToInsert.Add(check);
                    checkTime = checkTime.AddHours(4); // check every 4 hours
                }

                // Enforce final consecutive failures for Critical APIs so the evaluator rules remain valid
                if (sample.Status == ApiHealthStatus.Critical)
                {
                    // Ensure last 3 checks are failures
                    var last3 = checksToInsert.TakeLast(3).ToList();
                    foreach (var check in last3)
                    {
                        check.IsSuccessful = false;
                        check.StatusCode = 500;
                        check.ErrorMessage = "Connection reset by peer";
                        check.ResponseTimeMs = 0;
                        check.ResponseSizeBytes = 0;
                    }
                }

                dbContext.HealthChecks.AddRange(checksToInsert);
                dbContext.SaveChanges();
            }

            // -------------------------------------------------------------------------
            // 5. Seed Alerts
            // -------------------------------------------------------------------------
            logger.LogInformation("Seeding sample Alerts...");

            var criticalApis = dbContext.ApiEndpoints.Where(e => e.CurrentStatus == ApiHealthStatus.Critical).ToList();
            var warningApis = dbContext.ApiEndpoints.Where(e => e.CurrentStatus == ApiHealthStatus.Warning).ToList();
            var healthyApis = dbContext.ApiEndpoints.Where(e => e.CurrentStatus == ApiHealthStatus.Healthy).ToList();

            // 3 Open Critical alerts
            foreach (var api in criticalApis.Take(3))
            {
                dbContext.Alerts.Add(new Alert
                {
                    ApiEndpointId = api.Id,
                    Severity      = AlertSeverity.Critical,
                    Message       = $"API '{api.Name}' has exceeded its failure limit. 3 consecutive checks failed.",
                    GeneratedAt   = now.AddHours(-1),
                    Status        = AlertStatus.Open
                });
            }

            // 2 Open Warning alerts
            foreach (var api in warningApis.Take(2))
            {
                dbContext.Alerts.Add(new Alert
                {
                    ApiEndpointId = api.Id,
                    Severity      = AlertSeverity.Warning,
                    Message       = $"API '{api.Name}' has degraded performance. Average response time is above 1000ms.",
                    GeneratedAt   = now.AddHours(-2),
                    Status        = AlertStatus.Open
                });
            }

            // 5 Closed/Resolved historical alerts
            int resolvedCount = 0;
            foreach (var api in healthyApis.Take(5))
            {
                dbContext.Alerts.Add(new Alert
                {
                    ApiEndpointId = api.Id,
                    Severity      = resolvedCount % 2 == 0 ? AlertSeverity.Critical : AlertSeverity.Warning,
                    Message       = $"API '{api.Name}' threshold breached (Temporary network fluctuation).",
                    GeneratedAt   = now.AddDays(-5).AddHours(-resolvedCount),
                    ResolvedAt    = now.AddDays(-5).AddHours(-resolvedCount).AddMinutes(15),
                    Status        = AlertStatus.Closed
                });
                resolvedCount++;
            }

            dbContext.SaveChanges();
            logger.LogInformation("Database Seeding Completed Successfully.");
        }
    }
}
