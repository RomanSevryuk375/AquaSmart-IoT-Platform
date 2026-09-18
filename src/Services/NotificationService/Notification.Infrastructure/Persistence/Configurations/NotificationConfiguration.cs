using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Domain.Entities.Notification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.EcosystemId).IsRequired(false);
        builder.Property(x => x.Level).IsRequired();
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.IsRead).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsPublished).IsRequired();
        builder.Property(x => x.PublishedAt).IsRequired(false);

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.Level);
        builder.HasIndex(x => x.IsRead);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EcosystemId);
    }
}
