using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using Device.Infrastructure.Persistence.Converters;

namespace Device.Infrastructure.Persistence;

public sealed class DeviceDbContext(DbContextOptions<DeviceDbContext> options)
    : DbContext(options)
{
    public DbSet<Controller> Controllers => Set<Controller>();
    public DbSet<RelayCommand> RelayCommands => Set<RelayCommand>();
    public DbSet<Relay> Relays => Set<Relay>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeviceDbContext).Assembly);
        modelBuilder.ConfigureOutbox();

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddValueConverters();

        base.ConfigureConventions(configurationBuilder);
    }
}
