// src/Services/ControlService/ControlService.Infrastructure/Persistence/Configurations/EcosystemConfiguration.cs
using Contracts.Constants;
using Control.Domain.Entities;
using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class EcosystemConfiguration : IEntityTypeConfiguration<Ecosystem>
{
    public void Configure(EntityTypeBuilder<Ecosystem> builder)
    {
        builder.ToTable("ecosystems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                dbValue => Name.Create(dbValue).Value)
            .HasMaxLength(EcosystemConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.Volume)
            .HasConversion(
                volume => volume != null
                    ? volume.Value
                    : (double?)null,
                dbValue => dbValue.HasValue
                    ? Volume.Create(dbValue.Value).Value
                    : null)
            .IsRequired(false);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ControllerId).IsUnique();
    }
}
