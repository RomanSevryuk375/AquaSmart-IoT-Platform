using Contracts.Results;
using Notification.Application.DTOs.Notification;

namespace Notification.Application.Interfaces;

public interface INotificationService
{
    public Task<Result<NotificationResponseDto>> GetNotificationByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    public Task<Result> MarkNotificationAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken);
}
