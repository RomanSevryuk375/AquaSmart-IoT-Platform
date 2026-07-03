using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class VacationModeEntityConfiguration
    : IEntityTypeConfiguration<VacationModeEntity>
{
    public void Configure(EntityTypeBuilder<VacationModeEntity> builder)
    {
        builder.ToTable("vacations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CalculatedFeed).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
