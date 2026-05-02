using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Repositories;

public sealed class MaintenanceLogRepository(SystemDbContext dbContext)
    : BaseRepository<MaintenanceLogEntity>(dbContext), IMaintenanceLogRepository
{
}
