using BuildingBlocks.Domain.Results;
using Device.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Device.Application.Features.Mqtt;

public sealed class ProcessCommandAckHandler(
    IControllerRepository controllerRepository,
    IRelayCommandsRepository commandsRepository,
    IRelayRepository relayRepository,
    ILogger<ProcessCommandAckHandler> logger)
    : IRequestHandler<ProcessCommandAckCommand, Result>
{
    public async Task<Result> Handle(
        ProcessCommandAckCommand request,
        CancellationToken cancellationToken)
    {
        Result<MacAddress> macResult = MacAddress.Create(request.TopicMac);
        if (macResult.IsFailure)
        {
            logger.LogWarning("MQTT command ack ignored: invalid topic MAC {TopicMac}.", request.TopicMac);
            return Result.Success();
        }

        if (request.CommandId == Guid.Empty)
        {
            logger.LogWarning("MQTT command ack ignored for MAC {MacAddress}: empty CommandId.", macResult.Value.Value);
            return Result.Success();
        }

        Controller? controller = await controllerRepository.GetByMacAddressAsync(
            macResult.Value.Value, cancellationToken);
        if (controller is null)
        {
            logger.LogWarning("MQTT command ack ignored: unknown controller for MAC {MacAddress}.", macResult.Value.Value);
            return Result.Success();
        }

        RelayCommand? command = await commandsRepository.GetByIdAsync(request.CommandId, cancellationToken);
        if (command is null)
        {
            logger.LogWarning(
                "MQTT command ack ignored: command {CommandId} not found (Controller {ControllerId}).",
                request.CommandId,
                controller.Id);
            return Result.Success();
        }

        if (command.ControllerId != controller.Id)
        {
            logger.LogWarning(
                "MQTT command ack rejected: command {CommandId} does not belong to Controller {ControllerId}.",
                request.CommandId,
                controller.Id);
            return Result.Success();
        }

        if (command.IsTerminal)
        {
            logger.LogDebug(
                "MQTT command ack ignored (idempotent): command {CommandId} is already in terminal status {Status}.",
                command.Id,
                command.Status);
            return Result.Success();
        }

        string normalizedStatus = request.Status.Trim().ToLowerInvariant();
        if (string.Equals(normalizedStatus, "completed", StringComparison.Ordinal))
        {
            Relay? relay = await relayRepository.GetByIdAsync(command.RelayId, cancellationToken);
            if (relay is not null)
            {
                relay.SetState(command.TargetState, relay.UserId);
            }

            command.MarkAsCompleted();
            return Result.Success();
        }

        if (string.Equals(normalizedStatus, "failed", StringComparison.Ordinal))
        {
            string errorMessage = string.IsNullOrWhiteSpace(request.ErrorMessage)
                ? "Command execution failed on device."
                : request.ErrorMessage.Trim();

            command.MarkAsFailed(errorMessage);
            return Result.Success();
        }

        logger.LogWarning(
            "MQTT command ack ignored: unknown status '{Status}' for command {CommandId}.",
            request.Status,
            command.Id);

        return Result.Success();
    }
}
