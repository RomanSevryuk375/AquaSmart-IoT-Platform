using Contracts.Abstractions;

namespace Notification.Domain.Interfaces;

public interface INotificationRepository : IRepository<Entities.Notification>
{
    public Task<IReadOnlyList<Entities.Notification>> GetUnpublishedNotificationsAsync(
        CancellationToken cancellationToken);
}
