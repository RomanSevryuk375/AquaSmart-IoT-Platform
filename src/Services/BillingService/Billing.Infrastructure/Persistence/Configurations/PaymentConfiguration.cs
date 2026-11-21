using Billing.Domain.Entities;
using Billing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.InvoiceId).IsRequired(false);
        builder.Property(x => x.PaymentMethodId).IsRequired(false);
        builder.Property(x => x.OriginalPaymentId).IsRequired(false);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.OwnsOne(x => x.Amount, amount =>
        {
            amount.Property(p => p.Amount)
                .HasColumnName("amount")
                .HasPrecision(12, 2)
                .IsRequired();

            amount.Property(p => p.Currency)
                .HasColumnName("currency")
                .HasMaxLength(BillingMoney.CurrencyLength)
                .IsRequired();
        });
        builder.Navigation(x => x.Amount).IsRequired();

        builder.Property(x => x.RefundedAmount).IsRequired();
        builder.Property(x => x.Provider).IsRequired();
        builder.Property(x => x.GatewayTransactionId)
            .HasMaxLength(Payment.MaxGatewayTransactionIdLength)
            .IsRequired(false);
        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(Payment.MaxIdempotencyKeyLength)
            .IsRequired(false);
        builder.Property(x => x.ErrorCode)
            .HasMaxLength(Payment.MaxErrorCodeLength)
            .IsRequired(false);
        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(Payment.MaxErrorMessageLength)
            .IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<BillingCustomer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.OriginalPaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => x.OriginalPaymentId)
            .HasFilter("original_payment_id IS NOT NULL");
        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");
        builder.HasIndex(x => new { x.Provider, x.GatewayTransactionId })
            .IsUnique()
            .HasFilter("gateway_transaction_id IS NOT NULL");

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
