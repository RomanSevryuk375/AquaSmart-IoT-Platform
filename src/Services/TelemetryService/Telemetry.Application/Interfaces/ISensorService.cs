using Contracts.Events.SensorEvents;
using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface ISensorService
{
    Task<ConsumerResult> CreateSensorAsync(
        SensorCreatedEvent @event,
        CancellationToken cancellationToken);

    Task<ConsumerResult> DeletedSensorAsync(
        SensorDeletedEvent @event,
        CancellationToken cancellationToken);

    Task<ConsumerResult> UpdatedSensorAsync(
        SensorUpdatedEvent @event,
        CancellationToken cancellationToken);

    Task<ConsumerResult> SetSensorStateAsync(
        SensorStateChangedEvent @event,
        CancellationToken cancellationToken);

    Task<ConsumerResult> SetSensorNameAsync(
        SensorRenamedEvent sensor,
        CancellationToken cancellationToken);
}