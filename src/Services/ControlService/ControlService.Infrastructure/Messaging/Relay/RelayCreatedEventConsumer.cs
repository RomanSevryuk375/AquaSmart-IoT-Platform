using Contracts.Events.RelayEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Relay;

internal class RelayCreatedEventConsumer(IRelayService service) 
    : IConsumer<RelayCreatedEvent>
{
    public async Task Consume(ConsumeContext<RelayCreatedEvent> context)
    {
        await service.CreateRelayAsync(
            context.Message, context.CancellationToken);
    }
}
