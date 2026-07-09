using Contracts.Events.SensorEvents;
using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface ISensorService
{
    public Task<ConsumerResult> CreateSensorAsync(
        SensorCreatedEvent @event,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> DeletedSensorAsync(
        SensorDeletedEvent @event,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> UpdatedSensorAsync(
        SensorUpdatedEvent @event,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> SetSensorStateAsync(
        SensorStateChangedEvent @event,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> SetSensorNameAsync(
        SensorRenamedEvent sensor,
        CancellationToken cancellationToken);
}