using Contracts.Events.RelayEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Relay;

public class RelayUpdatedEventConsumer(IRelayService service)
    : IConsumer<RelayUpdatedEvent>
{
    public async Task Consume(ConsumeContext<RelayUpdatedEvent> context)
    {
        await service.UpdatedRelayAsync(
            context.Message, context.CancellationToken);
    }
}
