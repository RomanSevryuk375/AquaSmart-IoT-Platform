using Contracts.Constants;

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
            .HasMaxLength(SensorConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.ConnectionProtocol)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ConnectionAddress)
           .HasMaxLength(SensorConstants.ConnectionAddressLength)
           .IsRequired();

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
    }
}
