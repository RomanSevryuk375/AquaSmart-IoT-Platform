using Contracts.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                dbValue => EmailAddress.Create(dbValue).Value)
            .HasMaxLength(UserConstants.EmailLength)
            .IsRequired();

        builder.Property(x => x.TimeZone)
            .HasConversion(
                tz => tz.Value,
                dbValue => TimeZoneId.Create(dbValue).Value)
            .HasColumnName("time_zone")
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasConversion(
                phone => phone.Value,
                dbValue => PhoneNumber.Create(dbValue).Value)
            .HasMaxLength(UserConstants.PhoneNumberLength)
            .IsRequired();

        builder.Property(x => x.EmailEnable).IsRequired();
        builder.Property(x => x.TgEnable).IsRequired();
        builder.Property(x => x.TelegramChatId).IsRequired(false);
        builder.Property(x => x.IsNotifyEnabled).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.PhoneNumber).IsUnique();

        builder.HasMany<Ecosystem>()
            .WithOne()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Domain.Entities.Notification>()
            .WithOne()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Reminder>()
            .WithOne()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<MaintenanceLog>()
            .WithOne()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
