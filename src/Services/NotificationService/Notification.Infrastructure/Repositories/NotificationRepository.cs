using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Repositories;

public sealed class NotificationRepository(SystemDbContext dbContext)
    : BaseRepository<Domain.Entities.Notification>(dbContext), INotificationRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Notification>> GetUnpublishedNotificationsAsync(
        CancellationToken cancellationToken)
    {
        return await Context.Notifications
            .AsNoTracking()
            .Where(x => x.IsPublished == false)
            .Where(x => x.RetryCount < 5)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
