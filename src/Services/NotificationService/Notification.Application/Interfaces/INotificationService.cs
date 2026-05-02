using Contracts.Results;
using Notification.Application.DTOs.Notification;

namespace Notification.Application.Interfaces;

public interface INotificationService
{
    Task<Result<IReadOnlyList<NotificationResponseDto>>> GetAllNotificationsAsync(
        NotificationFilterDto filter, 
        int? skip, 
        int? take, 
        CancellationToken cancellationToken);

    Task<Result<NotificationResponseDto>> GetNotificationByIdAsync(
        Guid notificationId, 
        CancellationToken cancellationToken);

    Task<Result> MarkNotificationAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken);
}