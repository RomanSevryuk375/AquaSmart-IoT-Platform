using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure;

public class SystemDbContext(DbContextOptions<SystemDbContext> options) : DbContext(options)
{
    public DbSet<AutomationRuleEntity> Rules { get; set; }
    public DbSet<EcosystemEntity> Aquariums { get; set; }
    public DbSet<RelayEntity> Relays { get; set; }
    public DbSet<RuleConditionEntity> RuleConditions { get; set; }
    public DbSet<ScheduleEntity> Schedules { get; set; }
    public DbSet<SensorEntity> Sensors { get; set; }
    public DbSet<VacationModeEntity> Vacations { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemDbContext).Assembly);
    }
}
