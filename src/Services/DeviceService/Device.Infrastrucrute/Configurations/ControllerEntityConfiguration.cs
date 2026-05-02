using Contracts.Constants;
using Device.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Device.Infrastructure.Configurations;

public sealed class ControllerEntityConfiguration 
    : IEntityTypeConfiguration<ControllerEntity>
{
    public void Configure(EntityTypeBuilder<ControllerEntity> builder)
    {
        builder.ToTable("controllers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.MacAddress)
            .HasMaxLength(ControllerConstants.MacAddressLength)
            .IsRequired();

        builder.Property(x => x.DeviceTokenHash).IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(ControllerConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.IsOnline).IsRequired();
        builder.Property(x => x.LastSeenAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany<SensorEntity>()
            .WithOne()
            .HasForeignKey(s => s.ControllerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<RelayEntity>()
            .WithOne()
            .HasForeignKey(r => r.ControllerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<RelayCommandsQueueEntity>()
            .WithOne()
            .HasForeignKey(x => x.ControllerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.MacAddress).IsUnique();
        builder.HasIndex(x => x.DeviceTokenHash).IsUnique();
    }
}
