using Contracts.Events.RelayEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Relay;

public class RelayStateChangedComandConsumer(IRelayService service)
    : IConsumer<ChangeRelayStateCommand>
{
    public async Task Consume(ConsumeContext<ChangeRelayStateCommand> context)
    {
        await service.ChangedStateAsync(
            context.Message, context.CancellationToken);
    }
}
