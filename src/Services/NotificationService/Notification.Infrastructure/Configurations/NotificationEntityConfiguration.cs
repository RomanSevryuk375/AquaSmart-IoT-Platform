using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Configurations;

public class NotificationEntityConfiguration 
    : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.EcosystemId).IsRequired();

        builder.Property(x => x.Level)
            .HasConversion<int>()
            .IsRequired();
        builder.HasIndex(x => x.Level);

        builder.Property(x => x.Message)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(x => x.IsRead).IsRequired();
        builder.HasIndex(x => x.IsRead);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsPublished).IsRequired();
        builder.Property(x => x.PublishedAt).IsRequired(false);
    }
}
