using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayCreatedEventConsumer(IRelayService service)
    : IConsumer<RelayCreatedEvent>
{
    public async Task Consume(ConsumeContext<RelayCreatedEvent> context)
    {
        ConsumerResult result = await service.CreateRelayAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
