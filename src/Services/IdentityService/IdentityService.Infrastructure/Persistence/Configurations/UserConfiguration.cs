using Contracts.Constants;
using IdentityService.Domain.Entities;
using IdentityService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                dbValue => Name.Create(dbValue).Value)
            .HasMaxLength(CommonConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.TimeZone)
            .HasConversion(
                tz => tz.Value,
                dbValue => TimeZoneId.Create(dbValue).Value)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.PhoneNumber).HasMaxLength(20);

        builder.Property(x => x.SubscriptionEndDate).IsRequired();
        builder.Property(x => x.SubscriptionId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<Subscription>()
            .WithMany()
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
