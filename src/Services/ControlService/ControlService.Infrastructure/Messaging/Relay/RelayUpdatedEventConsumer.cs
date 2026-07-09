using AutoMapper;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Features.Relays.Commands.SyncRelayUpdated;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayUpdatedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<RelayUpdatedEvent>
{
    public async Task Consume(ConsumeContext<RelayUpdatedEvent> context)
    {
        SyncRelayUpdatedCommand command = mapper.Map<SyncRelayUpdatedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
