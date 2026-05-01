using Contracts.Events.EcosystemEvents;
using MassTransit;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Messaging;

public sealed class EcosystemCreatedEventConsumer(
    IEcosystemService service) : IConsumer<EcosystemCreatedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemCreatedEvent> context)
    {
        var result = await service.CreateAquariumFromEventAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
