using Device.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Persistence;

public sealed class SystemDbContext(DbContextOptions<SystemDbContext> options) 
    : DbContext(options)
{
    public DbSet<ControllerEntity> Controllers { get; set; }
    public DbSet<RelayCommandsQueueEntity> RelayCommands { get; set; }
    public DbSet<RelayEntity> Relays { get; set; }
    public DbSet<SensorEntity> Sensors { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemDbContext).Assembly);
    }
}
