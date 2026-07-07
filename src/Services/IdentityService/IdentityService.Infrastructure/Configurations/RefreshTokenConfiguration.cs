using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.TokenHash)
            .HasMaxLength(256)
            .IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.Property(x => x.IsUsed).IsRequired();
        builder.Property(x => x.IsRevoked).IsRequired();
        builder.Property(x => x.ExpiredAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
