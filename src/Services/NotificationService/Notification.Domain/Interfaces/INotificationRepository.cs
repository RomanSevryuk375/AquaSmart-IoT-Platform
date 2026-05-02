using Contracts.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface INotificationRepository : IRepository<NotificationEntity>
{
    Task<IReadOnlyList<NotificationEntity>> GetUnpublishedNotificationsAsync(
        CancellationToken cancellationToken);
}
