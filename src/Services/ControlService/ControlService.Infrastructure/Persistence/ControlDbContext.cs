using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using Control.Domain.Entities;
using Control.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence;

public class ControlDbContext(DbContextOptions<ControlDbContext> options) : DbContext(options)
{
    public DbSet<AutomationRule> Rules => Set<AutomationRule>();
    public DbSet<Ecosystem> Ecosystems => Set<Ecosystem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Relay> Relays => Set<Relay>();
    public DbSet<RuleCondition> RuleConditions => Set<RuleCondition>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<VacationMode> Vacations => Set<VacationMode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ControlDbContext).Assembly);
        modelBuilder.ConfigureOutbox();

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddValueConverters();

        base.ConfigureConventions(configurationBuilder);
    }
}
