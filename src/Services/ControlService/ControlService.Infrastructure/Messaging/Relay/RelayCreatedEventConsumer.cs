using AutoMapper;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Features.Relays.Commands.SyncRelayCreated;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayCreatedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<RelayCreatedEvent>
{
    public async Task Consume(ConsumeContext<RelayCreatedEvent> context)
    {
        SyncRelayCreatedCommand command = mapper.Map<SyncRelayCreatedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
