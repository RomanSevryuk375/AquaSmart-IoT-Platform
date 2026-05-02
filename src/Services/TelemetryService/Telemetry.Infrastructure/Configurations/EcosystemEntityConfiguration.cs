using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Configurations;

public sealed class EcosystemEntityConfiguration 
    : IEntityTypeConfiguration<EcosystemEntity>
{
    public void Configure(EntityTypeBuilder<EcosystemEntity> builder)
    {
        builder.ToTable("ecosystems");

        builder.HasKey(e => e.Id);

        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.ControllerId);
    }
}
