using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Configurations;

public class RelayEntityConfiguration : IEntityTypeConfiguration<RelayEntity>
{
    public void Configure(EntityTypeBuilder<RelayEntity> builder)
    {
        builder.ToTable("relays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EcosystemId).IsRequired();
        builder.HasIndex(x => x.EcosystemId);

        builder.Property(x => x.Purpose)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsManual).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
