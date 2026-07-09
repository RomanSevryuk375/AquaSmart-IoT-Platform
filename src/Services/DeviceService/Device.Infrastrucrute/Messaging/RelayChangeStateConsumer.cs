using Contracts.Events.RelayEvents;
using Contracts.Results;
using Device.Application.Features.RelayCommands.Command.SetRelayState;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Device.Infrastructure.Messaging;

public sealed class RelayChangeStateConsumer(
    ISender sender,
    ILogger<RelayChangeStateConsumer> logger)
    : IConsumer<ChangeRelayStateEvent>
{
    public async Task Consume(ConsumeContext<ChangeRelayStateEvent> context)
    {
        logger.LogInformation("Received {EventName} for Relay {RelayId}", nameof(ChangeRelayStateEvent), context.Message.RelayId);

        var command = new SetRelayStateCommand
        {
            ControllerId = context.Message.ControllerId,
            ExpireAt = context.Message.ExpireAt,
            RelayId = context.Message.RelayId,
            TargetState = context.Message.TargetState,
        };

        Result result = await sender.Send(command, context.CancellationToken);
        if (!result.IsSuccess && result.Error is not null)
        {
            var exception = new ConsumerException(result.Error.Message);
            logger.LogError(exception, "Error occurred processing {EventName} for Relay {RelayId}: {ErrorMessage}", nameof(ChangeRelayStateEvent), context.Message.RelayId, result.Error.Message);
            throw exception;
        }
    }
}
