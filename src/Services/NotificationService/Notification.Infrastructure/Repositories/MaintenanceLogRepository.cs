using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Repositories;

public sealed class MaintenanceLogRepository(NotificationDbContext dbContext)
    : BaseRepository<MaintenanceLogEntity>(dbContext), IMaintenanceLogRepository
{
}
