using Control.Domain.Entities;
using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Control.Infrastructure.Persistence.Configurations;

public sealed class VacationModeConfiguration : IEntityTypeConfiguration<VacationMode>
{
    public void Configure(EntityTypeBuilder<VacationMode> builder)
    {
        builder.ToTable("vacations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CalculatedFeed).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.DateRange)
            .HasConversion(
                dr => dr.ToString(),
                dbValue => DateRange.Parse(dbValue))
            .HasColumnName("date_range")
            .HasMaxLength(128)
            .IsRequired();
    }
}
