using System.Text.Json;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Presentation.Authorization;
using IdentityService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Domain.Entities.Subscription>
{
    private const JsonSerializerOptions? Options = null;

    public void Configure(EntityTypeBuilder<Domain.Entities.Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Price).IsRequired();

        builder.Property(x => x.Permissions)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, Options),
                v => JsonSerializer.Deserialize<List<string>>(v, Options) ?? new List<string>()
            ).IsRequired();

        builder.Property(x => x.DurationDays).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.AddSubscriptionData();
    }
}

internal static class Extensions
{
    public static EntityTypeBuilder<Domain.Entities.Subscription> AddSubscriptionData(
             this EntityTypeBuilder<Domain.Entities.Subscription> builder)
    {
        builder.HasData(
            new
            {
                Id = Guid.Parse(SubscriptionType.Free),
                Name = Name.Create("Free").Value,
                Price = Money.Create(0m).Value,
                DurationDays = SubscriptionType.FreeDuration,
                Permissions = new List<string>
                {
                    SubPermissions.TankRead,
                    SubPermissions.TankCreate,
                    SubPermissions.TankUpdate,
                    SubPermissions.TankDelete,
                    SubPermissions.TankLimit1,
                    SubPermissions.DeviceControl,
                    SubPermissions.AutoRuleCreate,
                    SubPermissions.AutoRuleLimit5,
                    SubPermissions.AutoScheduleCreate,
                    SubPermissions.TelemetryView,
                    SubPermissions.MaintenanceLogRead,
                    SubPermissions.MaintenanceLogWrite,
                    SubPermissions.AccountUpdate,
                    SubPermissions.AccountView,
                },
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Version = Guid.NewGuid()
            },
            new
            {
                Id = Guid.Parse(SubscriptionType.Professional),
                Name = Name.Create("Professional").Value,
                Price = Money.Create(9.99m).Value,
                DurationDays = SubscriptionType.ProfessionalDuration,
                Permissions = new List<string>
                {
                    SubPermissions.TankRead,
                    SubPermissions.TankCreate,
                    SubPermissions.TankUpdate,
                    SubPermissions.TankDelete,
                    SubPermissions.TankLimit10,
                    SubPermissions.DeviceControl,
                    SubPermissions.AutoRuleCreate,
                    SubPermissions.AutoRuleLimit10,
                    SubPermissions.AutoScheduleCreate,
                    SubPermissions.TelemetryView,
                    SubPermissions.AnalyticsHistory,
                    SubPermissions.TelegramAlerts,
                    SubPermissions.MaintenanceLogRead,
                    SubPermissions.MaintenanceLogWrite,
                    SubPermissions.ReminderManage,
                    SubPermissions.AccountUpdate,
                    SubPermissions.AccountView,
                },
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Version = Guid.NewGuid()
            },
            new
            {
                Id = Guid.Parse(SubscriptionType.Elite),
                Name = Name.Create("Elite").Value,
                Price = Money.Create(19.99m).Value,
                DurationDays = SubscriptionType.EliteDuration,
                Permissions = new List<string>
                {
                    SubPermissions.TankRead,
                    SubPermissions.TankCreate,
                    SubPermissions.TankUpdate,
                    SubPermissions.TankDelete,
                    SubPermissions.TankLimitUnlimited,
                    SubPermissions.DeviceControl,
                    SubPermissions.DeviceEditManual,
                    SubPermissions.AutoRuleCreate,
                    SubPermissions.AutoRuleUnlimited,
                    SubPermissions.AutoScheduleCreate,
                    SubPermissions.VacationMode,
                    SubPermissions.TelemetryView,
                    SubPermissions.AnalyticsHistory,
                    SubPermissions.DiagnosticsFull,
                    SubPermissions.DataRealtime,
                    SubPermissions.MaintenanceLogRead,
                    SubPermissions.MaintenanceLogWrite,
                    SubPermissions.ReminderManage,
                    SubPermissions.EmailAlerts,
                    SubPermissions.TelegramAlerts,
                },
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Version = Guid.NewGuid()
            }
        );

        return builder;
    }
}
