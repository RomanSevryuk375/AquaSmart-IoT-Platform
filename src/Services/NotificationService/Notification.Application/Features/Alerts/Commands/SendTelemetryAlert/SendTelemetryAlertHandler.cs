using Contracts.Enums;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Alerts.Commands.SendTelemetryAlert;

public sealed class SendTelemetryAlertHandler(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IEcosystemRepository ecosystemRepository,
    IDeviceMetadataEnricher metadataEnricher) : IRequestHandler<SendTelemetryAlertCommand, Result>
{
    public async Task<Result> Handle(SendTelemetryAlertCommand request, CancellationToken cancellationToken)
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

        Result<DeviceMetadataDto> enrichedMetadataResult = await metadataEnricher.EnrichAsync(
            null, request.SensorId, request.RelayId, cancellationToken);
        if (enrichedMetadataResult.IsFailure)
        {
            return Result.Failure(enrichedMetadataResult.Error);
        }

        string sensorName = enrichedMetadataResult.Value.SensorName;
        string relayName = enrichedMetadataResult.Value.RelayName;
        Result<Domain.Entities.Notification>? notificationResult = Domain.Entities.Notification.Create(
            notificationId: NewId.NextGuid(), request.UserId, request.EcosystemId,
            level: NotificationLevel.Critical,
            rawMessage: $"In aquarium {existingEcosystem.EcosystemName}," +
            $" sensor {sensorName} sent data {request.Value}, " +
            $"relay {relayName} responsible for this sensor " +
            $"is in state {request.RelayState} at {request.RecordedAt:HH:mm:ss}");

        if (notificationResult.IsFailure)
        {
            return Result.Failure(notificationResult.Error);
        }

        await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);

        return Result.Success();
    }
}
