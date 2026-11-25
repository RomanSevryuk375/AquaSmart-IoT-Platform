using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Enums;
using Device.Application.Interfaces;
using Device.Domain.Events.RelayCommandEvents;
using Microsoft.Extensions.Logging;

namespace Device.Application.Handlers;

public sealed class RelayCommandCreatedHandler(
    IControllerRepository controllerRepository,
    IRelayCommandsRepository commandsRepository,
    IMqttCommandPublisher mqttCommandPublisher,
    IUnitOfWork unitOfWork,
    ILogger<RelayCommandCreatedHandler> logger) : INotificationHandler<RelayCommandCreatedDomainEvent>
{
    public async Task Handle(
        RelayCommandCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        RelayCommand? command = await commandsRepository.GetByIdAsync(notification.CommandId, cancellationToken);
        if (command is null || command.IsTerminal || command.Status != CommandStatus.Pending)
        {
            return;
        }

        if (command.ExpireAt.HasValue && command.ExpireAt.Value <= DateTime.UtcNow)
        {
            command.Expire();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        Controller? controller = await controllerRepository.GetByIdAsync(notification.ControllerId, cancellationToken);
        if (controller is null)
        {
            logger.LogWarning(
                "Skipping MQTT push for RelayCommand {CommandId}: Controller {ControllerId} not found.",
                notification.CommandId,
                notification.ControllerId);
            return;
        }

        var payload = new RelayCommandMqttPayload
        {
            CommandId = command.Id,
            RelayId = command.RelayId,
            TargetState = command.TargetState,
            ExpireAt = command.ExpireAt,
            CreatedAt = command.CreatedAt,
        };

        bool published = await mqttCommandPublisher.PublishRelayCommandAsync(
            controller.MacAddress.Value, payload, cancellationToken);

        if (published)
        {
            command.MarkAsSent();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            logger.LogWarning(
                "MQTT broker unavailable for RelayCommand {CommandId} (Controller {ControllerId}). Command remains Pending for retry/fallback.",
                command.Id,
                controller.Id);
        }
    }
}
