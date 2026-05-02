using Device.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Device.Infrastructure.Configurations;

public sealed class RelayCommandsQueueEntityConfiguration
    : IEntityTypeConfiguration<RelayCommandsQueueEntity>
{
    public void Configure(EntityTypeBuilder<RelayCommandsQueueEntity> builder)
    {
        builder.ToTable("relay_command_queues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.RelayId).IsRequired();

        builder.Property(x => x.Action)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ExpireAt).IsRequired(false);
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired(false);
        builder.Property(x => x.ErrorMessage).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new 
        { 
            x.ControllerId, 
            x.Status 
        });
    }
}
