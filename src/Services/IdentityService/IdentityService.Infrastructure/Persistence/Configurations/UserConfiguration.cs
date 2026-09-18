using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.TimeZone).IsRequired();
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
