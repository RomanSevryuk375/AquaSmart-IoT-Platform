using Contracts.Events.TelemetryEvents;
using Contracts.Results;

namespace Control.Application.Interfaces;

public interface ITelemetryService
{
    public Task<ConsumerResult> ProcessTelemetryAsync(
        TelemetryReceivedEvent telemetry,
        CancellationToken cancellationToken);
}