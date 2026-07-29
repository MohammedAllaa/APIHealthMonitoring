using APIHealthMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIHealthMonitoring.Persistence.Data.Configurations;

/// <summary>
/// Fluent API configuration for the <see cref="ApiEndpoint"/> entity.
/// Defines constraints, indexes, column types, and relationships.
/// Applied automatically via <c>ApplyConfigurationsFromAssembly</c> in AppDbContext.
/// </summary>
public class ApiEndpointConfiguration : IEntityTypeConfiguration<ApiEndpoint>
{
    public void Configure(EntityTypeBuilder<ApiEndpoint> builder)
    {
        // -------------------------------------------------------------------------
        // Table
        // -------------------------------------------------------------------------

        builder.ToTable("ApiEndpoints");

        // -------------------------------------------------------------------------
        // Primary Key (inherited from BaseEntity)
        // -------------------------------------------------------------------------

        builder.HasKey(e => e.Id);

        // -------------------------------------------------------------------------
        // Properties
        // -------------------------------------------------------------------------

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(e => e.BaseUrl)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(e => e.HealthEndpoint)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(e => e.ServiceOwner)
               .IsRequired()
               .HasMaxLength(200);

        // Enum stored as int (default EF behavior)
        builder.Property(e => e.HttpMethod)
               .IsRequired();

        builder.Property(e => e.Environment)
               .IsRequired();

        builder.Property(e => e.ExpectedStatusCode)
               .IsRequired();

        builder.Property(e => e.TimeoutSeconds)
               .IsRequired();

        builder.Property(e => e.IntervalSeconds)
               .IsRequired();

        builder.Property(e => e.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        // -------------------------------------------------------------------------
        // Indexes
        // -------------------------------------------------------------------------

        // Unique constraint on Name — enforced at DB level in addition to service layer
        builder.HasIndex(e => e.Name)
               .IsUnique()
               .HasDatabaseName("IX_ApiEndpoints_Name_Unique");

        // Common filter columns get non-clustered indexes for performance
        builder.HasIndex(e => e.Environment)
               .HasDatabaseName("IX_ApiEndpoints_Environment");

        builder.HasIndex(e => e.IsActive)
               .HasDatabaseName("IX_ApiEndpoints_IsActive");

        builder.HasIndex(e => e.ServiceOwner)
               .HasDatabaseName("IX_ApiEndpoints_ServiceOwner");

        // -------------------------------------------------------------------------
        // Relationships
        // -------------------------------------------------------------------------

        // One-to-one: ApiEndpoint → MonitoringConfiguration
        builder.HasOne(e => e.MonitoringConfig)
               .WithOne(c => c.ApiEndpoint)
               .HasForeignKey<MonitoringConfiguration>(c => c.ApiEndpointId)
               .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: ApiEndpoint → HealthChecks
        builder.HasMany(e => e.HealthChecks)
               .WithOne(h => h.ApiEndpoint)
               .HasForeignKey(h => h.ApiEndpointId)
               .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: ApiEndpoint → Alerts
        builder.HasMany(e => e.Alerts)
               .WithOne(a => a.ApiEndpoint)
               .HasForeignKey(a => a.ApiEndpointId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
