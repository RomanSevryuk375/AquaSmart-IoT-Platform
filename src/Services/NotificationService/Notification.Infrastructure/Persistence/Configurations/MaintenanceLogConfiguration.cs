using System.Text.Json;
using Contracts.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceLogConfiguration : IEntityTypeConfiguration<MaintenanceLog>
{
    public void Configure(EntityTypeBuilder<MaintenanceLog> builder)
    {
        var serializerOptions = new JsonSerializerOptions { WriteIndented = false };

        builder.ToTable("maintenance_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.EcosystemId).IsRequired();
        builder.Property(x => x.ActionDate).IsRequired();

        builder.Property(x => x.Metrics)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, serializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, double>>(v, serializerOptions)
                     ?? new Dictionary<string, double>()
            )
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, double>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToDictionary(entry => entry.Key, entry => entry.Value)
            ));

        builder.Property(x => x.Notes)
            .HasMaxLength(MaintenanceLogConstants.Length)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EcosystemId);
    }
}
