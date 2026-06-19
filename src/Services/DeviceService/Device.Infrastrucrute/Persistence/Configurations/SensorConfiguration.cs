using Contracts.Constants;
using Device.Domain.ValueObjects;

namespace Device.Infrastructure.Persistence.Configurations;

public sealed class SensorConfiguration 
    : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Name)
            .HasConversion(
                vo => vo.Value,
                dbVal => DeviceName.Create(dbVal).Value)
            .HasMaxLength(CommonConstants.NameLength)
            .IsRequired();

        builder.ComplexProperty(x => x.ConnectionAddress, ca =>
        {
            ca.Property(p => p.Protocol)
                .HasConversion<int>()
                .IsRequired();

            ca.Property(p => p.Address)
                .HasMaxLength(CommonConstants.ConnectionAddressLength)
                .IsRequired();
        });

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.State)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(SensorConstants.UnitLength)
            .IsRequired();

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
    }
}
