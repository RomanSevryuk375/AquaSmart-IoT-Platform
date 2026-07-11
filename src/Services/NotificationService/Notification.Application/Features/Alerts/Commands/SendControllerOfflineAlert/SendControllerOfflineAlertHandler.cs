using Contracts.Enums;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Alerts.Commands.SendControllerOfflineAlert;

public sealed class SendControllerOfflineAlertHandler(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IEcosystemRepository ecosystemRepository,
    IDeviceMetadataEnricher metadataEnricher) : IRequestHandler<SendControllerOfflineAlertCommand, Result>
{
    public async Task<Result> Handle(SendControllerOfflineAlertCommand request, CancellationToken cancellationToken)
    {
        User? existingUser = await userRepository.GetByIdAsync(
            request.UserId, cancellationToken);
        if (existingUser is null)
        {
            return Result.Failure(Error.NotFound<User>(
                $"User {request.UserId} not found"));
        }

        Ecosystem? existingEcosystem = await ecosystemRepository.GetByUserIdAsync(
            existingUser.Id, cancellationToken);
        if (existingEcosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem {existingUser.Id} not found"));
        }

        Result<DeviceMetadataDto> enrichedMetadataResult = await metadataEnricher.EnrichAsync(
            request.ControllerId, null, null, cancellationToken);
        if (enrichedMetadataResult.IsFailure)
        {
            return Result.Failure(enrichedMetadataResult.Error);
        }

        string controllerName = enrichedMetadataResult.Value.ControllerName;
        Result<Domain.Entities.Notification>? notificationResult = Domain.Entities.Notification.Create(
            notificationId: NewId.NextGuid(), request.UserId, existingEcosystem.Id,
            level: NotificationLevel.Critical,
            rawMessage: $"Controller {controllerName} was last online at {request.LastSeenAt:HH:mm:ss}");
        if (notificationResult.IsFailure)
        {
            return Result.Failure(notificationResult.Error);
        }

        await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);

        return Result.Success();
    }
}
