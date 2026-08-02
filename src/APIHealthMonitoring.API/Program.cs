using APIHealthMonitoring.Infrastructure;
using APIHealthMonitoring.Infrastructure.HealthChecks.BackgroundServices;
using APIHealthMonitoring.Persistence;
using APIHealthMonitoring.Persistence.Data;
using Microsoft.OpenApi.Models;

namespace APIHealthMonitoring
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // -------------------------------------------------------------------------
            // Service Registration
            // -------------------------------------------------------------------------

            // Select connection string based on environment:
            //   Development → DefaultConnection (local SQL Server)
            //   All other environments (Production, Staging…) → RemoteConnection
            var connectionStringName = builder.Environment.IsDevelopment()
                ? "DefaultConnection"
                : "RemoteConnection";
            var connectionString = builder.Configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            // Registers AppDbContext (SQL Server, IdentityDbContext) and IUnitOfWork.
            builder.Services.AddPersistenceServices(connectionString);

            // Registers ASP.NET Core Identity, JWT Bearer authentication,
            // and all auth/user-management services.
            builder.Services.AddIdentityServices(builder.Configuration);

            // IHttpClientFactory — used by HealthCheckExecutor to issue HTTP probes
            builder.Services.AddHttpClient();

            // Module 4 — Background monitoring engine
            builder.Services.AddHostedService<MonitoringBackgroundService>();

            builder.Services.AddControllers();

            // -------------------------------------------------------------------------
            // Swagger / OpenAPI — with JWT Bearer security definition
            // -------------------------------------------------------------------------

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title   = "API Health Monitoring",
                    Version = "v1",
                    Description = "API Health Monitoring System — Security & Identity Module"
                });

                // Add the JWT Bearer security scheme so Swagger UI includes an
                // "Authorize" button for sending tokens with requests.
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name         = "Authorization",
                    Description  = "Enter: Bearer {your JWT token}",
                    In           = ParameterLocation.Header,
                    Type         = SecuritySchemeType.ApiKey,
                    Scheme       = "Bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id   = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    }
                };

                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, Array.Empty<string>() }
                });
            });

            // -------------------------------------------------------------------------
            // HTTP Pipeline Configuration
            // -------------------------------------------------------------------------

            var app = builder.Build();

            // -------------------------------------------------------------------------
            // Database Seeding — roles + default admin (idempotent)
            // -------------------------------------------------------------------------
            await DatabaseSeeder.SeedAsync(app.Services);

            // Enable Swagger in all environments (including production)
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            // Authentication MUST come before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}