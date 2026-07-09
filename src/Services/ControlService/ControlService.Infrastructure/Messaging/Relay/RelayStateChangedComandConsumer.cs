using AutoMapper;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Features.Relays.Commands.SyncRelayState;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayStateChangedComandConsumer(ISender sender, IMapper mapper)
    : IConsumer<ChangeRelayStateEvent>
{
    public async Task Consume(ConsumeContext<ChangeRelayStateEvent> context)
    {
        SyncRelayStateCommand command = mapper.Map<SyncRelayStateCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
