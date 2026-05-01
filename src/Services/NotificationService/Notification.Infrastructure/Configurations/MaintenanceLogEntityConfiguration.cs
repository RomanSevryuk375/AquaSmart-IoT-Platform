using Contracts.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;
using System.Text.Json;

namespace Notification.Infrastructure.Configurations;

public sealed class MaintenanceLogEntityConfiguration
    : IEntityTypeConfiguration<MaintenanceLogEntity>
{
    public void Configure(EntityTypeBuilder<MaintenanceLogEntity> builder)
    {
        builder.ToTable("maintenance_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.ActionDate).IsRequired();

        builder.Property(x => x.Metrics)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, null as JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, double>>(v, null as JsonSerializerOptions)
                    ?? new Dictionary<string, double>()
            )
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(MaintenanceLogConstants.Length)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EcosystemId);
    }
}
