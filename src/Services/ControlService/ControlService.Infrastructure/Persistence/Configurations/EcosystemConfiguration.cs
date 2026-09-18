using Control.Domain.Entities;
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
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Volume).IsRequired(false);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ControllerId).IsUnique();

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
