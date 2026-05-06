using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;

namespace Notification.Infrastructure;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<EcosystemEntity> Aquariums { get; set; }
    public DbSet<MaintenanceLogEntity> MaintenanceLogs { get; set; }
    public DbSet<NotificationEntity> Notifications { get; set; }
    public DbSet<ReminderEntity> Reminders { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
