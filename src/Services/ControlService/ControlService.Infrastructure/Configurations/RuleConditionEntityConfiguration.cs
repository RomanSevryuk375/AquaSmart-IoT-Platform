using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Configurations;

public sealed class RuleConditionEntityConfiguration 
    : IEntityTypeConfiguration<RuleConditionEntity>
{
    public void Configure(EntityTypeBuilder<RuleConditionEntity> builder)
    {
        builder.ToTable("rule_conditions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AutomationRuleId).IsRequired();
        builder.Property(x => x.SensorId).IsRequired();

        builder.Property(x => x.Condition)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Threshold)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(x => x.Hysteresis)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.SensorId);
        builder.HasIndex(x => x.AutomationRuleId);
    }
}

