using Contracts.Constants;
using Device.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Device.Infrastructure.Configurations;

public class RelayEntityConfiguration : IEntityTypeConfiguration<RelayEntity>
{
    public void Configure(EntityTypeBuilder<RelayEntity> builder)
    {
        builder.ToTable("relays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.PowerSensorId).IsRequired(false);

        builder.Property(x => x.Name)
            .HasMaxLength(RelayConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.ConnectionProtocol)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ConnectionAddress)
           .HasMaxLength(RelayConstants.ConnectionAddressLength)
           .IsRequired();

        builder.Property(x => x.IsNormalyOpen).IsRequired();

        builder.Property(x => x.Purpose)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.IsManual).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new 
        { 
            x.ControllerId, 
            x.PowerSensorId,
            x.ConnectionAddress
        }).IsUnique();

        builder.HasIndex(x => x.PowerSensorId)
            .IsUnique();

        builder.HasMany<RelayCommandsQueueEntity>()
            .WithOne()
            .HasForeignKey(x => x.RelayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<SensorEntity>()
            .WithOne()
            .HasForeignKey<RelayEntity>(x => x.PowerSensorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
