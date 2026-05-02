using Contracts.Events.RelayEvents;
using Device.Application.Interfaces;
using MassTransit;

namespace Device.Infrastructure.Messaging;

public class RelayChangeStateConsumer(IRelayCommandQueueService service)
    : IConsumer<ChangeRelayStateCommand>
{
    public async Task Consume(ConsumeContext<ChangeRelayStateCommand> context)
    {
        var result = await service.SetRelayStateAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}