using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class RelayConfiguration : IEntityTypeConfiguration<Relay>
{
    public void Configure(EntityTypeBuilder<Relay> builder)
    {
        builder.ToTable("relays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.PowerSensorId).IsRequired(false);
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Purpose).IsRequired();
        builder.Property(x => x.IsManual).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.EcosystemId);
        builder.HasIndex(x => x.PowerSensorId)
            .HasFilter("power_sensor_id IS NOT NULL");

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
