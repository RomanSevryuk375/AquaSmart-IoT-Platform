using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayDeletedEventConsumer(IRelayService service)
    : IConsumer<RelayDeletedEvent>
{
    public async Task Consume(ConsumeContext<RelayDeletedEvent> context)
    {
        ConsumerResult result = await service.DeletedRelayAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
