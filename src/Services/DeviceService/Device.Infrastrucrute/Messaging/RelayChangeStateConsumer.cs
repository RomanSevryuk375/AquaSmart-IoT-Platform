using Contracts.Events.RelayEvents;
using Contracts.Results;
using Device.Application.Features.RelayCommands.Command.SetRelayState;
using MediatR;

namespace Device.Infrastructure.Messaging;

public sealed class RelayChangeStateConsumer(ISender sender)
    : IConsumer<ChangeRelayStateEvent>
{
    public async Task Consume(ConsumeContext<ChangeRelayStateEvent> context)
    {
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
            throw new ConsumerException(result.Error.Message);
        }
    }
}
