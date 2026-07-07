using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IdentityService.Infrastructure;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        foreach (IMutableEntityType entity in builder.Model.GetEntityTypes())
        {
            string? tableName = entity.GetTableName();
            if (tableName != null && tableName.StartsWith("AspNet"))
            {
                entity.SetTableName(tableName.Substring(6).ToLower());
            }
        }
    }
}
