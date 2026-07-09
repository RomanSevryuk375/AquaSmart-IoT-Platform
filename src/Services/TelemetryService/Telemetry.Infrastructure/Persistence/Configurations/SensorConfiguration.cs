using Contracts.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;
using Telemetry.Domain.ValueObjects;

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

        builder.Property(x => x.Name)
            .HasConversion(
                vo => vo.Value,
                dbVal => DeviceName.Create(dbVal).Value)
            .HasMaxLength(CommonConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.State).HasConversion<int>().IsRequired();
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
