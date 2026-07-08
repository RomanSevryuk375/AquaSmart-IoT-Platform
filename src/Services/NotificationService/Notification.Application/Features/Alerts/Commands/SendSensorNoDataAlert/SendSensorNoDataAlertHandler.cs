using Contracts.Enums;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Alerts.Commands.SendSensorNoDataAlert;

public sealed class SendSensorNoDataAlertHandler(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IEcosystemRepository ecosystemRepository) : IRequestHandler<SendSensorNoDataAlertCommand, Result>
{
    public async Task<Result> Handle(SendSensorNoDataAlertCommand request, CancellationToken cancellationToken)
    {
        bool existingUser = await userRepository.ExistsAsync(
            request.UserId, cancellationToken);
        if (!existingUser)
        {
            return Result.Failure(Error.NotFound<User>(
                $"User {request.UserId} not found"));
        }

        Ecosystem? existingEcosystem = await ecosystemRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);
        if (existingEcosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem {request.UserId} not found"));
        }

        Result<Domain.Entities.Notification>? notificationResult = Domain.Entities.Notification.Create(
            notificationId: NewId.NextGuid(), request.UserId, request.EcosystemId,
            level: NotificationLevel.Critical,
            rawMessage: $"Sensor {request.SensorId} " +
            $"from aquarium {existingEcosystem.EcosystemName} did not send data " +
            $"at time {request.LastSeenAt:HH:mm:ss}");

        if (notificationResult.IsFailure)
        {
            return Result.Failure(notificationResult.Error);
        }

        await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);

        return Result.Success();
    }
}
