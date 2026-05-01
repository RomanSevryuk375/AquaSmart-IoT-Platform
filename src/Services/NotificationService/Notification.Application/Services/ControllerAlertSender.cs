using Contracts.Enums;
using Contracts.Events.ControllerEvents;
using Contracts.Results;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public sealed class ControllerAlertSender(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : IControllerAlertSender
{
    public async Task<ConsumerResult> SendControllerNotOnlineAlert(
        ControllerNotOnlineEvent controllerEvent,
        CancellationToken cancellationToken)
    {
        var existingUser = await userRepository
            .GetByIdAsync(controllerEvent.UserId, cancellationToken);

        if (existingUser is null)
        {
            return ConsumerResult
                .RetryableError($"User {controllerEvent.UserId} not found");
        }

        var existingEcosystem = await ecosystemRepository
            .GetByUserIdAsync(existingUser.Id, cancellationToken);

        if (existingEcosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {existingUser.Id} not found");
        }

        var (notification, errors) = NotificationEntity.Create(
            controllerEvent.UserId,
            existingEcosystem.Id,
            NotificationLevelEnum.Critical,
            $"Controller {controllerEvent.ControllerId} " +
            $"was last online at {controllerEvent.LastSeenAt:HH:mm:ss}");

        if (notification is null)
        {
            return ConsumerResult
                .FatalError($"Failed to create {nameof(NotificationEntity)}: {string.Join(", ", errors)}");
        }

        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
