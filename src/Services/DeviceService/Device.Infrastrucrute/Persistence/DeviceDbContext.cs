using Device.Infrastructure.Persistence.Outbox;

namespace Device.Infrastructure.Persistence;

public sealed class DeviceDbContext(DbContextOptions<DeviceDbContext> options)
    : DbContext(options)
{
    public DbSet<Controller> Controllers { get; set; }
    public DbSet<RelayCommand> RelayCommands { get; set; }
    public DbSet<Relay> Relays { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeviceDbContext).Assembly);
}
