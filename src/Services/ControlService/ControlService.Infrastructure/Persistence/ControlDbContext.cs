using Control.Domain.Entities;
using Control.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence;

public class ControlDbContext(DbContextOptions<ControlDbContext> options) : DbContext(options)
{
    public DbSet<AutomationRule> Rules { get; set; }
    public DbSet<Ecosystem> Aquariums { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<Relay> Relays { get; set; }
    public DbSet<RuleCondition> RuleConditions { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<VacationMode> Vacations { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ControlDbContext).Assembly);
    }
}
