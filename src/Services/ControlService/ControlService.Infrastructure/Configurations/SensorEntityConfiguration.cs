using Contracts.Constants;
using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Configurations;

public sealed class SensorEntityConfiguration 
    : IEntityTypeConfiguration<SensorEntity>
{
    public void Configure(EntityTypeBuilder<SensorEntity> builder)
    {
        builder.ToTable("sensors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EcosystemId).IsRequired();
        builder.HasIndex(x => x.EcosystemId);

        builder.Property(x => x.ControllerId).IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(SensorConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.State)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
