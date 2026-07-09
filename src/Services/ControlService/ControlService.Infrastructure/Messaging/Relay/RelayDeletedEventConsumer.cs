using AutoMapper;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Features.Relays.Commands.SyncRelayDeleted;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayDeletedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<RelayDeletedEvent>
{
    public async Task Consume(ConsumeContext<RelayDeletedEvent> context)
    {
        SyncRelayDeletedCommand command = mapper.Map<SyncRelayDeletedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
