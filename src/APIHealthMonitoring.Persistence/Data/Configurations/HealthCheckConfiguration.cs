using APIHealthMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIHealthMonitoring.Persistence.Data.Configurations;

/// <summary>
/// Fluent API configuration for <see cref="HealthCheck"/> records.
/// Health check records are immutable — no soft delete, no update tracking.
/// </summary>
public class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheck>
{
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable("HealthChecks");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.CheckedAt)
               .IsRequired();

        builder.Property(h => h.ResponseTimeMs)
               .IsRequired();

        builder.Property(h => h.IsSuccessful)
               .IsRequired();

        builder.Property(h => h.StatusCode)
               .IsRequired(false);

        builder.Property(h => h.ErrorMessage)
               .IsRequired(false)
               .HasMaxLength(500);

        builder.Property(h => h.ResponseSizeBytes)
               .IsRequired(false);

        // Lookup indexes for efficient history queries
        builder.HasIndex(h => h.ApiEndpointId)
               .HasDatabaseName("IX_HealthChecks_ApiEndpointId");

        builder.HasIndex(h => h.CheckedAt)
               .HasDatabaseName("IX_HealthChecks_CheckedAt");

        builder.HasIndex(h => new { h.ApiEndpointId, h.CheckedAt })
               .HasDatabaseName("IX_HealthChecks_ApiEndpointId_CheckedAt");

        builder.HasOne(h => h.ApiEndpoint)
               .WithMany(e => e.HealthChecks)
               .HasForeignKey(h => h.ApiEndpointId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
