using Contracts.Events.RelayEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Relay;

public class RelayDeletedEventConsumer(IRelayService service) 
    : IConsumer<RelayDeletedEvent>
{
    public async Task Consume(ConsumeContext<RelayDeletedEvent> context)
    {
        await service.DeletedRelayAsync(
            context.Message, context.CancellationToken);
    }
}
