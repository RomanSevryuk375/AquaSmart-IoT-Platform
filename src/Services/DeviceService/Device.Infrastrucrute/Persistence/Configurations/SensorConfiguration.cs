using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;

namespace Device.Infrastructure.Persistence.Configurations;

public sealed class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.ConnectionAddress).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.State).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(SensorConstants.UnitLength).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => new
        {
            x.ControllerId,
            x.ConnectionAddress
        }).IsUnique();

        builder.HasDiscriminator(x => x.Type)
            .HasValue<TemperatureSensor>(SensorType.Temperature)
            .HasValue<HumiditySensor>(SensorType.Humidity)
            .HasValue<PressureSensor>(SensorType.Pressure)
            .HasValue<VoltageSensor>(SensorType.Voltage);

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
