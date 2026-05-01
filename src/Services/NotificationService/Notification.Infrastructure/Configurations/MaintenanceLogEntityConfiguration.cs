using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Configurations;

public class MaintenanceLogEntityConfiguration
    : IEntityTypeConfiguration<MaintenanceLogEntity>
{
    public void Configure(EntityTypeBuilder<MaintenanceLogEntity> builder)
    {
        builder.ToTable("maintenance_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.EcosystemId).IsRequired();
        builder.HasIndex(x => x.EcosystemId);

        builder.Property(x => x.ActionDate).IsRequired();

        builder.Property(x => x.PhLevel)
            .HasPrecision(4, 2)
            .IsRequired(false);

        builder.Property(x => x.KhLevel)
            .HasPrecision(4, 2)
            .IsRequired(false);

        builder.Property(x => x.No3Level)
            .HasPrecision(4, 2)
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .HasMaxLength(1024)
            .IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
