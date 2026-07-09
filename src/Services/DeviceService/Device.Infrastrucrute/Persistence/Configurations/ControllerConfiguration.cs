using Device.Domain.ValueObjects;

namespace Device.Infrastructure.Persistence.Configurations;

public sealed class ControllerConfiguration
    : IEntityTypeConfiguration<Controller>
{
    public void Configure(EntityTypeBuilder<Controller> builder)
    {
        builder.ToTable("controllers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.MacAddress)
            .HasConversion(
                vo => vo.Value,
                dbVal => MacAddress.Create(dbVal).Value)
            .HasMaxLength(ControllerConstants.MacAddressLength)
            .IsRequired();

        builder.Property(x => x.DeviceTokenHash).IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                vo => vo.Value,
                dbVal => DeviceName.Create(dbVal).Value)
            .HasMaxLength(CommonConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.IsOnline).IsRequired();
        builder.Property(x => x.LastSeenAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany<Sensor>()
            .WithOne()
            .HasForeignKey(s => s.ControllerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Relay>()
            .WithOne()
            .HasForeignKey(r => r.ControllerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<RelayCommand>()
            .WithOne()
            .HasForeignKey(x => x.ControllerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.MacAddress).IsUnique();
        builder.HasIndex(x => x.DeviceTokenHash).IsUnique();

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
