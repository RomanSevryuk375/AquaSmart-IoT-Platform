using Contracts.Constants;
using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class AutomationRuleEntityConfiguration
    : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("automation_rules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.RelayId).IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(RuleConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.Operator)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Action)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Conditions)
            .WithOne()
            .HasForeignKey(x => x.AutomationRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EcosystemId);
        builder.HasIndex(x => x.IsActive);
    }
}
