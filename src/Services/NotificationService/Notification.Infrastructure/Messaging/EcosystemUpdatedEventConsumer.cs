using Contracts.Events.EcosystemEvents;
using MassTransit;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Messaging;

public sealed class EcosystemUpdatedEventConsumer(
    IEcosystemService service) : IConsumer<EcosystemUdatedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemUdatedEvent> context)
    {
        var result = await service.UpdateAquariumFromEventAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
