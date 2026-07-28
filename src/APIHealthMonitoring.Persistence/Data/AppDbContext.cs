using APIHealthMonitoring.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APIHealthMonitoring.Persistence.Data;

/// <summary>
/// The primary Entity Framework Core database context for the application.
/// Extends <see cref="IdentityDbContext{TUser, TRole, TKey}"/> to merge the
/// ASP.NET Core Identity schema (AspNetUsers, AspNetRoles, etc.) into the same
/// database as the rest of the application — a single migration surface.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of <see cref="AppDbContext"/>.
    /// Options (connection string, provider, etc.) are injected by the DI container.
    /// </summary>
    /// <param name="options">
    /// The EF Core configuration options, registered in Program.cs via AddDbContext.
    /// </param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // -------------------------------------------------------------------------
    // DbSets — each one maps to a database table
    // -------------------------------------------------------------------------

    // Identity tables (AspNetUsers, AspNetRoles, etc.) are exposed through the
    // base IdentityDbContext — no need to redeclare them here.

    // Module 2 — API Endpoint Registry
    public DbSet<ApiEndpoint>            ApiEndpoints            => Set<ApiEndpoint>();
    public DbSet<MonitoringConfiguration> MonitoringConfigurations => Set<MonitoringConfiguration>();
    public DbSet<HealthCheck>            HealthChecks            => Set<HealthCheck>();
    public DbSet<Alert>                  Alerts                  => Set<Alert>();

    // -------------------------------------------------------------------------
    // Model Configuration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Configures entity mappings, relationships, constraints, and indexes.
    /// Identity base schema is applied first via base.OnModelCreating,
    /// then application-specific configurations are applied from the assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANT: call base first so Identity creates its table structure
        // before any application-level IEntityTypeConfiguration is applied.
        base.OnModelCreating(modelBuilder);

        // Automatically discovers and applies all classes that implement
        // IEntityTypeConfiguration<T> in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // -------------------------------------------------------------------------
    // Audit Trail — automatic CreatedAt / UpdatedAt stamping
    // -------------------------------------------------------------------------

    /// <summary>
    /// Overrides SaveChangesAsync to automatically set audit fields
    /// on all entities that inherit from <see cref="BaseEntity"/>.
    /// This ensures no developer forgets to set these fields manually.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = null;
                    break;

                case EntityState.Modified:
                    // Never allow CreatedAt to be changed after insertion.
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}