using Contracts.Constants;
using Device.Domain.Entities;
using Device.Domain.Entities.Sensors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Device.Infrastructure.Persistence.Configurations;

public sealed class RelayEntityConfiguration 
    : IEntityTypeConfiguration<Relay>
{
    public void Configure(EntityTypeBuilder<Relay> builder)
    {
        builder.ToTable("relays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
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

        builder.Property(x => x.IsNormallyOpen).IsRequired();

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

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.PowerSensorId)
            .IsUnique();

        builder.HasMany<RelayCommand>()
            .WithOne()
            .HasForeignKey(x => x.RelayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Sensor>()
            .WithOne()
            .HasForeignKey<Relay>(x => x.PowerSensorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
