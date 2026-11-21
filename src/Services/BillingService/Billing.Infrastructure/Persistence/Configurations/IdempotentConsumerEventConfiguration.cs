using Billing.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class IdempotentConsumerEventConfiguration : IEntityTypeConfiguration<IdempotentConsumerEvent>
{
    public void Configure(EntityTypeBuilder<IdempotentConsumerEvent> builder)
    {
        builder.ToTable("idempotent_consumer_events");

        builder.HasKey(x => new { x.EventId, x.ConsumerName });

        builder.Property(x => x.EventId)
            .HasMaxLength(IdempotentConsumerEvent.MaxEventIdLength)
            .IsRequired();

        builder.Property(x => x.ConsumerName)
            .HasMaxLength(IdempotentConsumerEvent.MaxConsumerNameLength)
            .IsRequired();

        builder.Property(x => x.ProcessedAt).IsRequired();

        builder.HasIndex(x => x.ProcessedAt);
    }
}
