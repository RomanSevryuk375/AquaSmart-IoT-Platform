using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class BillingCustomerConfiguration : IEntityTypeConfiguration<BillingCustomer>
{
    public void Configure(EntityTypeBuilder<BillingCustomer> builder)
    {
        builder.ToTable("billing_customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Email).IsRequired();
        builder.Property(x => x.GatewayCustomerId)
            .HasMaxLength(BillingCustomer.MaxGatewayCustomerIdLength)
            .IsRequired(false);
        builder.Property(x => x.DefaultPaymentMethodId).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.PaymentMethods)
            .WithOne()
            .HasForeignKey(pm => pm.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.PaymentMethods)
            .HasField("_paymentMethods");

        builder.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey(x => x.DefaultPaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.GatewayCustomerId)
            .IsUnique()
            .HasFilter("gateway_customer_id IS NOT NULL");

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
