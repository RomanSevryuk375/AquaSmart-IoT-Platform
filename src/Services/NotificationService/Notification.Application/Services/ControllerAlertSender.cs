using Contracts.Enums;
using Contracts.Events.ControllerEvents;
using Contracts.Results;
using MassTransit;
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
        User? existingUser = await userRepository
            .GetByIdAsync(controllerEvent.UserId, cancellationToken);

        if (existingUser is null)
        {
            return ConsumerResult
                .RetryableError($"User {controllerEvent.UserId} not found");
        }

        Ecosystem? existingEcosystem = await ecosystemRepository
            .GetByUserIdAsync(existingUser.Id, cancellationToken);

        if (existingEcosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {existingUser.Id} not found");
        }

        Result<Domain.Entities.Notification>? notificationResult = Domain.Entities.Notification.Create(
            NewId.NextGuid(),
            controllerEvent.UserId,
            existingEcosystem.Id,
            NotificationLevel.Critical,
            $"Controller {controllerEvent.ControllerId} " +
            $"was last online at {controllerEvent.LastSeenAt:HH:mm:ss}");

        if (notificationResult.IsFailure)
        {
            return ConsumerResult.FatalError(notificationResult.Error.Message);
        }

        await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
