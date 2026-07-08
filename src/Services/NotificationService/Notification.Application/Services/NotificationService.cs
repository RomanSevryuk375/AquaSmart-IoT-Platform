using AutoMapper;
using Contracts.Results;
using Notification.Application.DTOs.Notification;
using Notification.Application.Interfaces;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    IMapper mapper) : INotificationService
{
    public async Task<Result<NotificationResponseDto>> GetNotificationByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Notification? notification = await notificationRepository
            .GetByIdAsync(notificationId, cancellationToken);


        if (notification is null)
        {
            return Result<NotificationResponseDto>
                .Failure(Error.NotFound(
                    "Notification.NotFound",
                    $"Notification {notificationId} not found"));
        }

        if (notification.UserId != userContext.UserId)
        {
            return Result<NotificationResponseDto>
                .Failure(Error.Validation(
                    "Access.Denied",
                    $"Invalid user id"));
        }

        return Result<NotificationResponseDto>.Success(
            mapper.Map<NotificationResponseDto>(notification));
    }

    public async Task<Result> MarkNotificationAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Notification? notification = await notificationRepository
            .GetByIdAsync(notificationId, cancellationToken);

        if (notification is null ||
            notification.UserId != userContext.UserId)
        {
            return Result.Failure(Error.NotFound(
                    "Notification.NotFound",
                    $"Notification {notificationId} not found"));
        }

        if (notification.IsRead)
        {
            return Result.Success();
        }

        notification.MarkAsRead();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
