using Contracts.Enums;
using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using MassTransit;
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
            $"In aquarium {existingEcosystem.EcosystemName}," +
            $" sensor {alertEvent.SensorId} sent data {alertEvent.Value}, " +
            $"relay responsible for this sensor " +
            $"is in state {alertEvent.RelayState} at {alertEvent.RecordedAt:HH:mm:ss}");

        if (notificationResult.IsFailure)
        {
            return ConsumerResult.FatalError(notificationResult.Error.Message);
        }

        await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
