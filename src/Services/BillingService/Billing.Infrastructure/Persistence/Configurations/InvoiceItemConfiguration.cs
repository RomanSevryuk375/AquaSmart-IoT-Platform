using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceId).IsRequired();
        builder.Property(x => x.Description)
            .HasMaxLength(InvoiceItem.MaxDescriptionLength)
            .IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.UnitPrice).IsRequired();
        builder.Property(x => x.TotalAmount).IsRequired();

        builder.OwnsOne(x => x.Period, period =>
        {
            period.Property(p => p.StartDate)
                .HasColumnName("period_start");

            period.Property(p => p.EndDate)
                .HasColumnName("period_end");
        });
        builder.Navigation(x => x.Period).IsRequired(false);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.InvoiceId);
    }
}
