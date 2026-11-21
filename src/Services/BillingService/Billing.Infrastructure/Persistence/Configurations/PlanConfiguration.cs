using Billing.Domain.Entities;
using Billing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubscriptionTypeId).IsRequired();
        builder.Property(x => x.Code)
            .HasMaxLength(Plan.MaxCodeLength)
            .IsRequired();
        builder.Property(x => x.Name)
            .HasMaxLength(Plan.MaxNameLength)
            .IsRequired();

        builder.OwnsOne(x => x.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("price")
                .HasPrecision(12, 2)
                .IsRequired();

            price.Property(p => p.Currency)
                .HasColumnName("currency")
                .HasMaxLength(BillingMoney.CurrencyLength)
                .IsRequired();
        });
        builder.Navigation(x => x.Price).IsRequired();

        builder.Property(x => x.BillingInterval).IsRequired();
        builder.Property(x => x.IntervalCount).IsRequired();
        builder.Property(x => x.TrialPeriodDays).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.SubscriptionTypeId)
            .HasFilter("is_active = TRUE");

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
