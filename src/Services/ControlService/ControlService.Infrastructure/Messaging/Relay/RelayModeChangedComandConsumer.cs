using AutoMapper;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Features.Relays.Commands.SyncRelayMode;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayModeChangedComandConsumer(ISender sender, IMapper mapper)
    : IConsumer<RelayModeChangedEvent>
{
    public async Task Consume(ConsumeContext<RelayModeChangedEvent> context)
    {
        SyncRelayModeCommand command = mapper.Map<SyncRelayModeCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
