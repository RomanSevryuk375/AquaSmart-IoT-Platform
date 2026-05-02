using Contracts.Events.EcosystemEvents;
using MassTransit;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Messaging;

public sealed class EcosystemDeletedEventConsumer(
    IEcosystemService service) : IConsumer<EcosystemDeletedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemDeletedEvent> context)
    {
        var result = await service.DeleteAquariumFromEventAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
