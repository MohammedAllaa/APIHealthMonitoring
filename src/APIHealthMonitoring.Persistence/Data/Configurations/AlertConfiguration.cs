using APIHealthMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIHealthMonitoring.Persistence.Data.Configurations;

/// <summary>
/// Fluent API configuration for <see cref="Alert"/> entities.
/// </summary>
public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Severity)
               .IsRequired();

        builder.Property(a => a.Message)
               .IsRequired()
               .HasMaxLength(1000);

        builder.Property(a => a.GeneratedAt)
               .IsRequired();

        builder.Property(a => a.ResolvedAt)
               .IsRequired(false);

        builder.Property(a => a.Status)
               .IsRequired();

        // Indexes for efficient alert queries
        builder.HasIndex(a => a.ApiEndpointId)
               .HasDatabaseName("IX_Alerts_ApiEndpointId");

        builder.HasIndex(a => a.Status)
               .HasDatabaseName("IX_Alerts_Status");

        builder.HasIndex(a => new { a.ApiEndpointId, a.Status, a.Severity })
               .HasDatabaseName("IX_Alerts_ApiEndpointId_Status_Severity");

        builder.HasOne(a => a.ApiEndpoint)
               .WithMany(e => e.Alerts)
               .HasForeignKey(a => a.ApiEndpointId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
