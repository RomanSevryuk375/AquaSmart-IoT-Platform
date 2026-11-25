using BuildingBlocks.Domain.Results;
using Device.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Device.Application.Features.Mqtt;

public sealed class ProcessRelayStateSyncHandler(
    IControllerRepository controllerRepository,
    IRelayRepository relayRepository,
    IRelayCommandsRepository commandsRepository,
    ILogger<ProcessRelayStateSyncHandler> logger)
    : IRequestHandler<ProcessRelayStateSyncCommand, Result>
{
    public async Task<Result> Handle(
        ProcessRelayStateSyncCommand request,
        CancellationToken cancellationToken)
    {
        Result<MacAddress> macResult = MacAddress.Create(request.TopicMac);
        if (macResult.IsFailure)
        {
            logger.LogWarning("MQTT relays/state ignored: invalid topic MAC {TopicMac}.", request.TopicMac);
            return Result.Success();
        }

        string normalizedSource = request.Source.Trim().ToLowerInvariant();
        if (normalizedSource is not ("manual" or "boot" or "sync"))
        {
            logger.LogWarning(
                "MQTT relays/state ignored for MAC {MacAddress}: unsupported source '{Source}'.",
                macResult.Value.Value,
                request.Source);
            return Result.Success();
        }

        if (request.Relays.Count == 0)
        {
            return Result.Success();
        }

        Controller? controller = await controllerRepository.GetByMacAddressAsync(
            macResult.Value.Value, cancellationToken);
        if (controller is null)
        {
            logger.LogWarning(
                "MQTT relays/state ignored: unknown controller for MAC {MacAddress}.",
                macResult.Value.Value);
            return Result.Success();
        }

        IReadOnlyList<Relay> controllerRelays = await relayRepository.GetAllByControllerId(
            controller.Id, cancellationToken);
        Dictionary<Guid, Relay> relaysById = controllerRelays.ToDictionary(r => r.Id);

        bool isManualOverride = string.Equals(normalizedSource, "manual", StringComparison.Ordinal);

        foreach (MqttRelayStateItemDto item in request.Relays)
        {
            if (item.RelayId == Guid.Empty || !relaysById.TryGetValue(item.RelayId, out Relay? relay))
            {
                logger.LogWarning(
                    "MQTT relays/state skipped unknown or foreign RelayId {RelayId} for Controller {ControllerId} (MAC: {MacAddress}).",
                    item.RelayId,
                    controller.Id,
                    controller.MacAddress.Value);
                continue;
            }

            if (isManualOverride)
            {
                relay.SyncStateFromDevice(item.State, relay.UserId);
                relay.SyncModeFromDevice(item.IsManual, relay.UserId);

                IReadOnlyList<RelayCommand> activeCommands = await commandsRepository.GetActiveByRelayIdAsync(
                    relay.Id, cancellationToken);
                foreach (RelayCommand activeCommand in activeCommands)
                {
                    activeCommand.Cancel();
                }

                if (activeCommands.Count > 0)
                {
                    logger.LogInformation(
                        "Manual Override Wins: cancelled {CancelledCount} active commands for Relay {RelayId} (Controller {ControllerId}).",
                        activeCommands.Count,
                        relay.Id,
                        controller.Id);
                }
            }
            else
            {
                relay.SyncStateFromDevice(item.State, relay.UserId);
            }
        }

        return Result.Success();
    }
}
