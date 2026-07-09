using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Persistence.Configurations;

public class TelemetryAggregateEntityConfiguration
    : IEntityTypeConfiguration<AggregateTelemetry>
{
    public void Configure(EntityTypeBuilder<AggregateTelemetry> builder)
    {
        builder.ToTable("telemetry_aggregate_data");

        builder.HasKey(t => t.Id);

        builder.Property(x => x.SensorId).IsRequired();
        builder.Property(x => x.PeriodStart).IsRequired();
        builder.Property(x => x.Period)
            .HasConversion<int>()
            .IsRequired();

        builder.ComplexProperty(x => x.Summary, a =>
        {
            a.Property(x => x.MinValue).HasPrecision(10, 4).IsRequired();
            a.Property(x => x.MaxValue).HasPrecision(10, 4).IsRequired();
            a.Property(x => x.AvgValue).HasPrecision(10, 4).IsRequired();
            a.Property(x => x.Count).IsRequired();
        });

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsAggregated).IsRequired();

        builder.HasIndex(x => x.IsAggregated);

        builder.HasIndex(t => new
        {
            t.SensorId,
            t.Period,
            t.PeriodStart
        });

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
