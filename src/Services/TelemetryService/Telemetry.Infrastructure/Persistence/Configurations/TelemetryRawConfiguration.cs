using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Persistence.Configurations;

public sealed class TelemetryRawConfiguration
    : IEntityTypeConfiguration<RawTelemetry>
{
    public void Configure(EntityTypeBuilder<RawTelemetry> builder)
    {
        builder.ToTable("telemetry_raw_data");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SensorId).IsRequired();
        builder.Property(x => x.Value).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.ExternalMessageId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RecordedAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsAggregated).IsRequired();

        builder.HasIndex(x => x.ExternalMessageId).IsUnique();
        builder.HasIndex(x => x.IsAggregated);
        builder.HasIndex(x => new
        {
            x.SensorId,
            x.RecordedAt
        });

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
