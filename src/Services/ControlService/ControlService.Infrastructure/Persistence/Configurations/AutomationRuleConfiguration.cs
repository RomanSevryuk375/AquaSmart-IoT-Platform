using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("automation_rules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.RelayId).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Operator).IsRequired();
        builder.Property(x => x.Action).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Conditions)
            .WithOne()
            .HasForeignKey(x => x.AutomationRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Conditions)
            .HasField("_ruleConditions");

        builder.HasIndex(x => x.EcosystemId);
        builder.HasIndex(x => x.IsActive);

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
