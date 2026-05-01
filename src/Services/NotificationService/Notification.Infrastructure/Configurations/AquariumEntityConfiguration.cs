using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Configurations;

public class AquariumEntityConfiguration
    : IEntityTypeConfiguration<EcosystemEntity>
{
    public void Configure(EntityTypeBuilder<EcosystemEntity> builder)
    {
        builder.ToTable("aquariums");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany<NotificationEntity>()
            .WithOne()
            .HasForeignKey(n => n.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ReminderEntity>()
            .WithOne()
            .HasForeignKey(n => n.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<MaintenanceLogEntity>()
            .WithOne()
            .HasForeignKey(n => n.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
