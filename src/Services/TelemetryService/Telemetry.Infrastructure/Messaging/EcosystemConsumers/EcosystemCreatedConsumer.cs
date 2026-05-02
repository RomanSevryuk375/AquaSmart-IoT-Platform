using Contracts.Events.EcosystemEvents;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging.EcosystemConsumers;

public sealed class EcosystemCreatedConsumer(
    IEcosystemService service) : IConsumer<Contracts.Events.EcosystemEvents.EcosystemCreatedEvent>
{
    public async Task Consume(ConsumeContext<Contracts.Events.EcosystemEvents.EcosystemCreatedEvent> context)
    {
        var result = await service.CreateEcosystemAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable) 
        {
            throw new Exception(result.Error);
        }
    }
}
