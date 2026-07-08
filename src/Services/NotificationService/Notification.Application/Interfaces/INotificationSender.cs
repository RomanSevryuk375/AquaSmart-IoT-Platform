using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface INotificationSender
{
    Task ProcessSingleNotificationAsync(
        Domain.Entities.Notification notification, CancellationToken cancellationToken);
}