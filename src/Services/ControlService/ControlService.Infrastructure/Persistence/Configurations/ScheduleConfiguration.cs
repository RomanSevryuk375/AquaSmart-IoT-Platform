using Control.Domain.Entities;
using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedules");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.RelayId).IsRequired();

        builder.Property(x => x.CronExpression)
            .HasConversion(
                cron => cron.Value,
                dbValue => CronSchedule.Load(dbValue))
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.DurationMin).IsRequired();
        builder.Property(x => x.IsFadeMode).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
