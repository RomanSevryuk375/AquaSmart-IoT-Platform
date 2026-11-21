using Billing.Domain.Entities;
using Billing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Provider).IsRequired();
        builder.Property(x => x.GatewayToken)
            .HasMaxLength(PaymentMethod.MaxGatewayTokenLength)
            .IsRequired();

        builder.OwnsOne(x => x.CardDetails, card =>
        {
            card.Property(c => c.CardLast4)
                .HasColumnName("card_last4")
                .HasMaxLength(CardDetails.Last4Length);

            card.Property(c => c.CardBrand)
                .HasColumnName("card_brand")
                .HasMaxLength(CardDetails.MaxBrandLength);

            card.Property(c => c.ExpMonth)
                .HasColumnName("exp_month");

            card.Property(c => c.ExpYear)
                .HasColumnName("exp_year");
        });
        builder.Navigation(x => x.CardDetails).IsRequired(false);

        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.CustomerId, "ix_payment_methods_customer_id");
        builder.HasIndex(x => x.CustomerId, "ix_payment_methods_customer_active")
            .HasFilter("is_active = TRUE");
        builder.HasIndex(x => new { x.CustomerId, x.Provider, x.GatewayToken })
            .IsUnique()
            .HasFilter("is_active = TRUE");

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
