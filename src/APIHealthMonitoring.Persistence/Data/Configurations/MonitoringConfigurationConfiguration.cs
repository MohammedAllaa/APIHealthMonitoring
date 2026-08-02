using APIHealthMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIHealthMonitoring.Persistence.Data.Configurations;

/// <summary>
/// Fluent API configuration for the <see cref="MonitoringConfiguration"/> entity.
/// Specifies constraints, indexes, defaults, and navigation properties.
/// </summary>
public class MonitoringConfigurationConfiguration : IEntityTypeConfiguration<MonitoringConfiguration>
{
    public void Configure(EntityTypeBuilder<MonitoringConfiguration> builder)
    {
        builder.ToTable("MonitoringConfigurations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SlowThresholdMs)
               .IsRequired()
               .HasDefaultValue(1000);

        builder.Property(c => c.CriticalThresholdMs)
               .IsRequired()
               .HasDefaultValue(2000);

        builder.Property(c => c.FailureCountLimit)
               .IsRequired()
               .HasDefaultValue(3);

        builder.Property(c => c.AvailabilityThreshold)
               .IsRequired()
               .HasPrecision(5, 2)
               .HasDefaultValue(99.0m);

        // Enforce 1:1 unique constraint on ApiEndpointId at database level
        builder.HasIndex(c => c.ApiEndpointId)
               .IsUnique()
               .HasDatabaseName("IX_MonitoringConfigurations_ApiEndpointId_Unique");

        builder.HasOne(c => c.ApiEndpoint)
               .WithOne(e => e.MonitoringConfig)
               .HasForeignKey<MonitoringConfiguration>(c => c.ApiEndpointId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
