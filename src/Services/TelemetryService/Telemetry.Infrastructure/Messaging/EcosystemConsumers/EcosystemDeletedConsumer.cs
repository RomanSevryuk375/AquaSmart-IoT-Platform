using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging.EcosystemConsumers;

public sealed class EcosystemDeletedConsumer(
    IEcosystemService service) : IConsumer<EcosystemDeletedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemDeletedEvent> context)
    {
        ConsumerResult result = await service.DeleteEcosystemAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
