using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Telemetry.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageConfiguration
: IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Content)
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(x => x.OccurredOnUtc).IsRequired();
        builder.Property(x => x.ProcessedOnUtc).IsRequired(false);
        builder.Property(x => x.Error).IsRequired(false);

        builder.HasIndex(x => x.OccurredOnUtc);
    }
}
