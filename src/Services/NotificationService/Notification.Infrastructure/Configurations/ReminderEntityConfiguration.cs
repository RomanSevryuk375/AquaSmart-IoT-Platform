using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Configurations;

public class ReminderEntityConfiguration
    : IEntityTypeConfiguration<ReminderEntity>
{
    public void Configure(EntityTypeBuilder<ReminderEntity> builder)
    {
        builder.ToTable("reminders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.EcosystemId).IsRequired();

        builder.Property(x => x.TaskName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.IntervalDays).IsRequired();

        builder.Property(x => x.NextDueAt).IsRequired();
        builder.HasIndex(x => x.NextDueAt);

        builder.Property(x => x.LastNotifiedAt).IsRequired(false);
        builder.Property(x => x.LastDoneAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
