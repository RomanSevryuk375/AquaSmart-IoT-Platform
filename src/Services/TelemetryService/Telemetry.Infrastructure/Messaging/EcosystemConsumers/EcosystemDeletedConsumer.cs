using Contracts.Events.EcosystemEvents;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging.EcosystemConsumers;

public sealed class EcosystemDeletedConsumer(
    IEcosystemService service) : IConsumer<EcosystemDeletedEvent>
{
    public async Task Consume(ConsumeContext<EcosystemDeletedEvent> context)
    {
        var result = await service.DeleteEcosystemAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
