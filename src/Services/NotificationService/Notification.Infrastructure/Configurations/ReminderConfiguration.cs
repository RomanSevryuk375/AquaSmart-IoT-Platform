using Contracts.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Configurations;

public sealed class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("reminders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.EcosystemId).IsRequired();

        builder.Property(x => x.TaskName)
            .HasConversion(
                name => name.Value,
                dbValue => Name.Create(dbValue).Value)
            .HasColumnName("task_name")
            .HasMaxLength(ReminderConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.IntervalDays).IsRequired();
        builder.Property(x => x.NextDueAt).IsRequired();
        builder.Property(x => x.LastNotifiedAt).IsRequired(false);
        builder.Property(x => x.LastDoneAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.NextDueAt);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EcosystemId);
    }
}
