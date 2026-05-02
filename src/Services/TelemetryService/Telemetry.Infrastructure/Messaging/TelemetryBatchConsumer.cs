using Contracts.Events.TelemetryEvents;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging;

public class TelemetryBatchConsumer(
    ITelemetryDataService dataService) : IConsumer<TelemetryBatchEvent>
{
    public async Task Consume(ConsumeContext<TelemetryBatchEvent> context)
    {
        var result = await dataService.AddDataAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
