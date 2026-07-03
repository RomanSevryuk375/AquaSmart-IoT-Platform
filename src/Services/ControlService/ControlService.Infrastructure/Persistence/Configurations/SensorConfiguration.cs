using Contracts.Constants;
using Control.Domain.Entities;
using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class SensorConfiguration
    : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EcosystemId).IsRequired();
        builder.HasIndex(x => x.EcosystemId);

        builder.Property(x => x.ControllerId).IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                dbValue => Name.Create(dbValue).Value)
            .HasMaxLength(EcosystemConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.State)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.LastValue)
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
