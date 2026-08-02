using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Persistence.Data;
using APIHealthMonitoring.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APIHealthMonitoring.Persistence;

/// <summary>
/// Contains the Dependency Injection registration extension method
/// for all Persistence layer services.
/// Called once from Program.cs at application startup.
/// Keeps infrastructure wiring inside the Persistence layer,
/// away from the API entry point.
/// </summary>
public static class PersistenceServiceRegistration
{
    /// <summary>
    /// Registers all Persistence layer services with the DI container.
    /// Includes the EF Core DbContext, Unit of Work, and connection string configuration.
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <param name="configuration">
    /// The application configuration, used to read the connection string
    /// from appsettings.json.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance to support
    /// method chaining in Program.cs.
    /// </returns>
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string connectionString)
    {
        // -------------------------------------------------------------------------
        // Database Context
        // -------------------------------------------------------------------------

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    // Tells EF Core which assembly contains the migration files.
                    // This is important when migrations live in a separate project
                    // from the DbContext (which is our case).
                    sqlServerOptions.MigrationsAssembly(
                        typeof(AppDbContext).Assembly.FullName);
                });
        });

        // -------------------------------------------------------------------------
        // Unit of Work
        // -------------------------------------------------------------------------

        // Scoped: one instance per HTTP request.
        // This ensures that all repositories within a single request
        // share the same DbContext instance and transaction boundary.
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // -------------------------------------------------------------------------
        // Custom Repositories
        // -------------------------------------------------------------------------

        services.AddScoped<IApiEndpointRepository, ApiEndpointRepository>();

        return services;
    }
}