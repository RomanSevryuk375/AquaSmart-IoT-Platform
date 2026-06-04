using Contracts.Events.RelayEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayUpdatedEventConsumer(IRelayService service)
    : IConsumer<RelayUpdatedEvent>
{
    public async Task Consume(ConsumeContext<RelayUpdatedEvent> context)
    {
        var result = await service.UpdatedRelayAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
