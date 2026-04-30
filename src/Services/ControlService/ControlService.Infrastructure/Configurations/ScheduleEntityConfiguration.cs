using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Configurations;

public sealed class ScheduleEntityConfiguration 
    : IEntityTypeConfiguration<ScheduleEntity>
{
    public void Configure(EntityTypeBuilder<ScheduleEntity> builder)
    {
        builder.ToTable("schedules");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.RelayId).IsRequired();

        builder.Property(x => x.CronExpression)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.DurationMin).IsRequired();
        builder.Property(x => x.IsFadeMode).IsRequired();
        builder.Property(x => x.IsEnable).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
