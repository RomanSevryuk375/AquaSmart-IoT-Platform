using BuildingBlocks.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Persistence.Configurations;

public sealed class SensorConfiguration
    : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.State).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(SensorConstants.NameLength).IsRequired();
        builder.Property(x => x.LastValue).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDataDelayed).IsRequired();

        builder.HasMany<RawTelemetry>()
            .WithOne()
            .HasForeignKey(x => x.SensorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<AggregateTelemetry>()
            .WithOne()
            .HasForeignKey(x => x.SensorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EcosystemId);

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
