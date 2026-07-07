using System.Text.Json;
using Contracts.Authorization;
using Contracts.Constants;
using IdentityService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Domain.Entities.Subscription>
{
    private const JsonSerializerOptions? Options = null;

    public void Configure(EntityTypeBuilder<Domain.Entities.Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                dbValue => Name.Create(dbValue).Value)
            .HasMaxLength(CommonConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(
                money => money.Amount,
                dbValue => Money.Create(dbValue).Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Permissions)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, Options),
                v => JsonSerializer.Deserialize<List<string>>(v, Options) ?? new List<string>()
            )
            .IsRequired();

        builder.Property(x => x.DurationDays).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasData(
            new
            {
                Id = Guid.Parse(Contracts.Enums.Subscription.Free),
                Name = Name.Create("Free").Value,
                Price = Money.Create(0m).Value,
                DurationDays = Contracts.Enums.Subscription.FreeDuration,
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
                Id = Guid.Parse(Contracts.Enums.Subscription.Professional),
                Name = Name.Create("Professional").Value,
                Price = Money.Create(9.99m).Value,
                DurationDays = Contracts.Enums.Subscription.ProfessionalDuration,
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
                Id = Guid.Parse(Contracts.Enums.Subscription.Elite),
                Name = Name.Create("Elite").Value,
                Price = Money.Create(19.99m).Value,
                DurationDays = Contracts.Enums.Subscription.EliteDuration,
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
    }
}
