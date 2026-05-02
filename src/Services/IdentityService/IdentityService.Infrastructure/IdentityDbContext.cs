using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<SubscriptionEntity> Subscriptions { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName != null && tableName.StartsWith("AspNet"))
            {
                entity.SetTableName(tableName.Substring(6).ToLower());
            }
        }
    }
}
