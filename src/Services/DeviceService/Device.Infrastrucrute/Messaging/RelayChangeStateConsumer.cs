using Contracts.Events.RelayEvents;
using Contracts.Results;
using Device.Application.Interfaces;

namespace Device.Infrastructure.Messaging;

public sealed class RelayChangeStateConsumer(IRelayCommandQueueService service)
    : IConsumer<ChangeRelayStateEvent>
{
    public async Task Consume(ConsumeContext<ChangeRelayStateEvent> context)
    {
        ConsumerResult result = await service.SetRelayStateAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess &&
            result.IsRetryable &&
            result.Error is not null)
        {
            throw new ConsumerException(result.Error);
        }
    }
}
