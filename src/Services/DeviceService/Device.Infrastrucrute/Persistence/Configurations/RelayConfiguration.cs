using Device.Domain.ValueObjects;

namespace Device.Infrastructure.Persistence.Configurations;

public sealed class RelayConfiguration
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
            .HasConversion(
                vo => vo.Value,
                dbVal => DeviceName.Create(dbVal).Value)
            .HasMaxLength(CommonConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.ConnectionAddress)
            .HasConversion(
                vo => vo.ToString(),
                dbVal => ConnectionAddress.Parse(dbVal))
            .HasColumnName("connection_address")
            .HasMaxLength(64)
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

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
