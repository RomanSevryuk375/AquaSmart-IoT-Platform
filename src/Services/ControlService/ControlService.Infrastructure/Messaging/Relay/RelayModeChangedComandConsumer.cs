using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Relay;

internal sealed class RelayModeChangedComandConsumer(IRelayService service)
    : IConsumer<RelayModeChangedEvent>
{
    public async Task Consume(ConsumeContext<RelayModeChangedEvent> context)
    {
        ConsumerResult result = await service.ChangedModeAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
