using Contracts.Enums;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public class SensorAlertSender(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : ISensorAlertSender
{
    public async Task<ConsumerResult> SendSensorNoDataAlertAsync(
        SensorNoDataAlertEvent alertEvent,
        CancellationToken cancellationToken)
    {
        bool existingUser = await userRepository
            .ExistsAsync(alertEvent.UserId, cancellationToken);

        if (!existingUser)
        {
            return ConsumerResult
                .RetryableError($"User {alertEvent.UserId} not found");
        }

        Ecosystem? existingEcosystem = await ecosystemRepository
            .GetByIdAsync(alertEvent.EcosytemId, cancellationToken);

        if (existingEcosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {alertEvent.EcosytemId} not found");
        }

        Result<Domain.Entities.Notification>? notificationResult = Domain.Entities.Notification.Create(
            NewId.NextGuid(),
            alertEvent.UserId,
            alertEvent.EcosytemId,
            NotificationLevel.Critical,
            $"Sensor {alertEvent.SensorId} " +
            $"from aquarium {existingEcosystem.EcosystemName} did not send data " +
            $"at time {alertEvent.LastSeenAt:HH:mm:ss}");

        if (notificationResult.IsFailure)
        {
            return ConsumerResult.FatalError(notificationResult.Error.Message);
        }

        await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
