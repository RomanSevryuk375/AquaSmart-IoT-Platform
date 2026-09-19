using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class RuleConditionConfiguration : IEntityTypeConfiguration<RuleCondition>
{
    public void Configure(EntityTypeBuilder<RuleCondition> builder)
    {
        builder.ToTable("rule_conditions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.AutomationRuleId).IsRequired();
        builder.Property(x => x.SensorId).IsRequired();
        builder.Property(x => x.Condition).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ConditionThreshold).IsRequired();

        builder.HasIndex(x => x.SensorId);
        builder.HasIndex(x => x.AutomationRuleId);
    }
}
