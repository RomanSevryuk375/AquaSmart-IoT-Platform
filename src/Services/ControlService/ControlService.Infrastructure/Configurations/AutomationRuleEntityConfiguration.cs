using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Configurations;

internal class AutomationRuleEntityConfiguration
    : IEntityTypeConfiguration<AutomationRuleEntity>
{
    public void Configure(EntityTypeBuilder<AutomationRuleEntity> builder)
    {
        builder.ToTable("automation_rules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EcosystemId).IsRequired();

        builder.Property(x => x.SensorId).IsRequired();
        builder.HasIndex(x => x.SensorId);

        builder.Property(x => x.RelayId).IsRequired();
        builder.HasIndex(x => x.RelayId);

        builder.Property(x => x.Condition)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Threshold)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Hysteresis)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
