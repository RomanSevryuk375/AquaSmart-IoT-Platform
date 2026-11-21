using Billing.Domain.Entities;
using Billing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.PlanId).IsRequired();
        builder.Property(x => x.PaymentMethodId).IsRequired(false);
        builder.Property(x => x.Status).IsRequired();

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

        builder.OwnsOne(x => x.Period, period =>
        {
            period.Property(p => p.StartDate)
                .HasColumnName("current_period_start")
                .IsRequired();

            period.Property(p => p.EndDate)
                .HasColumnName("current_period_end")
                .IsRequired();

            period.HasIndex(p => p.EndDate);
        });
        builder.Navigation(x => x.Period).IsRequired();

        builder.Property(x => x.TrialEnd).IsRequired(false);
        builder.Property(x => x.CancelAtPeriodEnd).IsRequired();
        builder.Property(x => x.CanceledAt).IsRequired(false);
        builder.Property(x => x.FailedPaymentAttempts).IsRequired();
        builder.Property(x => x.NextRetryAt).IsRequired(false);
        builder.Property(x => x.GatewaySubscriptionId)
            .HasMaxLength(Subscription.MaxGatewaySubscriptionIdLength)
            .IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<BillingCustomer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.CustomerId, "ix_subscriptions_customer_id");
        builder.HasIndex(x => x.CustomerId, "ux_subscriptions_customer_active")
            .IsUnique()
            .HasFilter("status IN (1, 2, 3)");
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.NextRetryAt)
            .HasFilter("status = 3 AND next_retry_at IS NOT NULL");
        builder.HasIndex(x => x.TrialEnd)
            .HasFilter("trial_end IS NOT NULL");
        builder.HasIndex(x => x.GatewaySubscriptionId)
            .IsUnique()
            .HasFilter("gateway_subscription_id IS NOT NULL");

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
