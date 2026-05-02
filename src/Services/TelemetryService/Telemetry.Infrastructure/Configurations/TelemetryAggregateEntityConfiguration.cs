using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Configurations;

public class TelemetryAggregateEntityConfiguration
    : IEntityTypeConfiguration<TelemetryAggregateEntity>
{
    public void Configure(EntityTypeBuilder<TelemetryAggregateEntity> builder)
    {
        builder.ToTable("telemetry_aggregate_data");

        builder.HasKey(t => t.Id);

        builder.Property(x => x.SensorId).IsRequired();
        builder.Property(x => x.PeriodStart).IsRequired();
        builder.Property(x => x.Period)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.MinValue)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(x => x.MaxValue)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(x => x.AvgValue)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(x => x.DataPointsCount).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsAggregated).IsRequired();

        builder.HasIndex(x => x.IsAggregated);

        builder.HasIndex(t => new
        {
            t.SensorId,
            t.Period,
            t.PeriodStart
        });
    }
}
