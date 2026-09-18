using Control.Domain.Entities;
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
        builder.Property(x => x.CronExpression).IsRequired();
        builder.Property(x => x.DurationMin).IsRequired();
        builder.Property(x => x.IsFadeMode).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
