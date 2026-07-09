using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class EcosystemConfiguration : IEntityTypeConfiguration<Ecosystem>
{
    public void Configure(EntityTypeBuilder<Ecosystem> builder)
    {
        builder.ToTable("ecosystems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.EcosystemName)
            .HasConversion(
                name => name.Value,
                dbValue => Name.Create(dbValue).Value)
            .HasColumnName("name")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.UserId);

        builder.HasMany<Domain.Entities.Notification>()
            .WithOne()
            .HasForeignKey(n => n.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Reminder>()
            .WithOne()
            .HasForeignKey(n => n.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<MaintenanceLog>()
            .WithOne()
            .HasForeignKey(n => n.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
