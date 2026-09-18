using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.Persistence.Converters;

namespace Telemetry.Infrastructure.Persistence;

public class TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : DbContext(options)
{
    public DbSet<Ecosystem> Ecosystems => Set<Ecosystem>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<AggregateTelemetry> TelemetryAggregateData => Set<AggregateTelemetry>();
    public DbSet<RawTelemetry> TelemetryRawData => Set<RawTelemetry>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelemetryDbContext).Assembly);
        modelBuilder.ConfigureOutbox();

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddValueConverters();

        base.ConfigureConventions(configurationBuilder);
    }
}
