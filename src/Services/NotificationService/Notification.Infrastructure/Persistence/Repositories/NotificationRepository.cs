using Microsoft.EntityFrameworkCore;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(NotificationDbContext dbContext)
    : BaseRepository<Domain.Entities.Notification>(dbContext), INotificationRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Notification>> GetUnpublishedNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Notifications
            .AsNoTracking()
            .Where(x => !x.IsPublished)
            .Where(x => x.RetryCount < 5)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
