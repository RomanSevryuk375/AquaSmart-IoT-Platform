using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure;

public class SystemDbContext(DbContextOptions<SystemDbContext> options) : DbContext(options)
{
    public DbSet<EcosystemEntity> Ecosystems { get; set; }
    public DbSet<SensorEntity> Sensors { get; set; }
    public DbSet<TelemetryAggregateEntity> TelemetryAggregateData { get; set; }
    public DbSet<TelemetryRawEntity> TelemetryRawData { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemDbContext).Assembly);
    }
}
