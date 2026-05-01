using Contracts.Enums;
using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public class TelemetryAlertSender(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : ITelemetryAlertSender
{
    public async Task<ConsumerResult> SendTelemetryAlertAsync(
        CriticalTelemetryThresholdAlertEvent alertEvent,
        CancellationToken cancellationToken)
    {
        var existingUser = await userRepository
            .ExistsAsync(alertEvent.UserId, cancellationToken);

        if (!existingUser)
        {
            return ConsumerResult
                .RetryableError($"User {alertEvent.UserId} not found");
        }

        var existingEcosystem = await ecosystemRepository
            .GetByIdAsync(alertEvent.EcosytemId, cancellationToken);

        if (existingEcosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {alertEvent.EcosytemId} not found");
        }

        var (notification, errors) = NotificationEntity.Create(
            alertEvent.UserId,
            alertEvent.EcosytemId,
            NotificationLevelEnum.Critical,
            $"In aquarium {existingEcosystem.Name}," +
            $" sensor {alertEvent.SensorId} sent data {alertEvent.Value}, " +
            $"relay responsible for this sensor " +
            $"is in state {alertEvent.RelayState} at {alertEvent.RecordedAt:HH:mm:ss}");

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
