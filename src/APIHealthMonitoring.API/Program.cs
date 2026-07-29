using APIHealthMonitoring.API.Middleware;
using APIHealthMonitoring.Infrastructure;
using APIHealthMonitoring.Infrastructure.HealthChecks.BackgroundServices;
using APIHealthMonitoring.Persistence;
using APIHealthMonitoring.Persistence.Data;
using Microsoft.OpenApi.Models;
using Serilog;

namespace APIHealthMonitoring
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Bootstrap logger for early startup logging before DI container is built
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting API Health Monitoring Host...");

                var builder = WebApplication.CreateBuilder(args);

                // Module 11 — Serilog Integration
                builder.Host.UseSerilog((ctx, services, cfg) => cfg
                    .ReadFrom.Configuration(ctx.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

                // -------------------------------------------------------------------------
                // Service Registration
                // -------------------------------------------------------------------------

                // Registers AppDbContext (SQL Server, IdentityDbContext) and IUnitOfWork.
                builder.Services.AddPersistenceServices(builder.Configuration);

                // Registers ASP.NET Core Identity, JWT Bearer authentication,
                // and all auth/user-management services.
                builder.Services.AddIdentityServices(builder.Configuration);

                // IHttpClientFactory — used by HealthCheckExecutor to issue HTTP probes
                builder.Services.AddHttpClient();

                // Module 4 — Background monitoring engine
                builder.Services.AddHostedService<MonitoringBackgroundService>();

                // Module 9 — Global exception handling (RFC 7807 ProblemDetails)
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
                builder.Services.AddProblemDetails();

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

                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Name         = "Authorization",
                        Description  = "Enter:  {your JWT token}",
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

                    options.AddSecurityDefinition("", securityScheme);
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        { securityScheme, Array.Empty<string>() }
                    });
                });

                // -------------------------------------------------------------------------
                // HTTP Pipeline Configuration
                // -------------------------------------------------------------------------

                var app = builder.Build();

                // Database Seeding — roles + default admin (idempotent)
                await DatabaseSeeder.SeedAsync(app.Services);

                // Module 11 — Correlation ID & Request Logging
                app.UseMiddleware<CorrelationIdMiddleware>();
                app.UseSerilogRequestLogging();

                // Must be placed to catch all downstream pipeline exceptions
                app.UseExceptionHandler();

                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                // Authentication MUST come before Authorization
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "API Health Monitoring Host terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}