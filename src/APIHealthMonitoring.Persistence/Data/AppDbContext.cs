using APIHealthMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIHealthMonitoring.Persistence.Data;

/// <summary>
/// The primary Entity Framework Core database context for the application.
/// Acts as the single unit of work and repository factory provided by EF Core.
/// All database access flows through this class.
/// </summary>
public class AppDbContext : DbContext
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

    // TODO: Add your DbSet<YourEntity> properties here as entities are created.
    // Example:
    //   public DbSet<MonitoredApi> MonitoredApis => Set<MonitoredApi>();

    // -------------------------------------------------------------------------
    // Model Configuration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Configures entity mappings, relationships, constraints, and indexes.
    /// Scans the assembly for all IEntityTypeConfiguration implementations
    /// and applies them automatically — no manual registration needed.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Automatically discovers and applies all classes that implement
        // IEntityTypeConfiguration<T> in this assembly.
        // This keeps AppDbContext clean — each entity owns its own configuration.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
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