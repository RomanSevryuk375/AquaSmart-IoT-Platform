using Contracts.Events.RelayEvents;
using Device.Application.Interfaces;

namespace Device.Infrastructure.Messaging;

public sealed class RelayChangeStateConsumer(IRelayCommandQueueService service)
    : IConsumer<ChangeRelayStateEvent>
{
    public async Task Consume(ConsumeContext<ChangeRelayStateEvent> context)
    {
        var result = await service.SetRelayStateAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}