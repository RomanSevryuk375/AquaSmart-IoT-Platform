using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IdentityService.Infrastructure;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    private const int SubStringSize = 6;

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        builder.ConfigureOutbox();

        foreach (IMutableEntityType entity in builder.Model.GetEntityTypes())
        {
            string? tableName = entity.GetTableName();
            if (tableName != null && tableName.StartsWith("AspNet"))
            {
                entity.SetTableName(tableName[SubStringSize..].ToLower());
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddValueConverters();

        base.ConfigureConventions(configurationBuilder);
    }
}
