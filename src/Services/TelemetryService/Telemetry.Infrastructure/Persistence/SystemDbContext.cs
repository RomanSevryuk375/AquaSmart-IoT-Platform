using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.Persistence.Outbox;

namespace Telemetry.Infrastructure.Persistence;

public class SystemDbContext(DbContextOptions<SystemDbContext> options) : DbContext(options)
{
    public DbSet<Ecosystem> Ecosystems { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<AggregateTelemetry> TelemetryAggregateData { get; set; }
    public DbSet<RawTelemetry> TelemetryRawData { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemDbContext).Assembly);
}
