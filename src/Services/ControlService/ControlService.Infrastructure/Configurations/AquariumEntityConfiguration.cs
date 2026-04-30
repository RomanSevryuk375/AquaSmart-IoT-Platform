using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Configurations;

public class AquariumEntityConfiguration : IEntityTypeConfiguration<EcosystemEntity>
{
    public void Configure(EntityTypeBuilder<EcosystemEntity> builder)
    {
        builder.ToTable("aquariums");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ControllerId).IsRequired();
        builder.HasIndex(x => x.ControllerId).IsUnique();   

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany<AutomationRuleEntity>()
            .WithOne()
            .HasForeignKey(a => a.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<RelayEntity>()
            .WithOne()
            .HasForeignKey(r => r.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ScheduleEntity>()
            .WithOne()
            .HasForeignKey(s => s.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<SensorEntity>()
            .WithOne()
            .HasForeignKey(s => s.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<VacationModeEntity>()
            .WithOne()
            .HasForeignKey(v => v.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
