using Contracts.Events.SensorEvents;
using Contracts.Results;

namespace Control.Application.Interfaces;

public interface ISensorService
{
    public Task<ConsumerResult> ChangedStateAsync(
        SensorStateChangedEvent sensor,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> CreateSensorAsync(
        SensorCreatedEvent newSensor,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> DeletedSensorAsync(
        SensorDeletedEvent sensor,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> HandleSensorNoDataEventAsync(
        SensorNoDataEvent sensor,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> UpdatedSensorAsync(
        SensorUpdatedEvent sensorUpdated,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> SetSensorNameAsync(
        SensorRenamedEvent sensor,
        CancellationToken cancellationToken);
}