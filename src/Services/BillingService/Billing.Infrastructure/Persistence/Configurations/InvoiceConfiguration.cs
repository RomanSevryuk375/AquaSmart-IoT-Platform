using Billing.Domain.Entities;
using Billing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.SubscriptionId).IsRequired(false);
        builder.Property(x => x.Number).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Subtotal).IsRequired();
        builder.Property(x => x.Discount).IsRequired();
        builder.Property(x => x.Tax).IsRequired();
        builder.Property(x => x.Total).IsRequired();
        builder.Property(x => x.Currency)
            .HasMaxLength(BillingMoney.CurrencyLength)
            .IsRequired();
        builder.Property(x => x.DueDate).IsRequired();
        builder.Property(x => x.PaidAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<BillingCustomer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Subscription>()
            .WithMany()
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .HasField("_items");

        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.SubscriptionId);

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
