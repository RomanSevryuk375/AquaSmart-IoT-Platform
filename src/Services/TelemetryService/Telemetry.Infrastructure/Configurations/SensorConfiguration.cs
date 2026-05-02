using Contracts.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Configurations;

public sealed class SensorConfiguration 
    : IEntityTypeConfiguration<SensorEntity>
{
    public void Configure(EntityTypeBuilder<SensorEntity> builder)
    {
        builder.ToTable("sensors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.EcosystemId).IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(SensorConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.State)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(SensorConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.LastValue)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDataDelayed).IsRequired();

        builder.HasIndex(x => x.EcosystemId);

        builder.HasMany<TelemetryRawEntity>()
            .WithOne()
            .HasForeignKey(x => x.SensorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<TelemetryAggregateEntity>()
            .WithOne()
            .HasForeignKey(x => x.SensorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
