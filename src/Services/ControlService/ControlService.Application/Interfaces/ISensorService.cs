using Contracts.Events.SensorEvents;
using Contracts.Results;

namespace Control.Application.Interfaces;

public interface ISensorService
{
    Task<ConsumerResult> ChangedStateAsync(
        SensorStateChangedEvent sensor, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> CreateSensorAsync(
        SensorCreatedEvent newSensor, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> DeletedSensorAsync(
        SensorDeletedEvent sensor, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> HandleSensorNoDataEventAsync(
        SensorNoDataEvent sensor, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> UpdatedSensorAsync(
        SensorUpdatedEvent sensorUpdated, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> SetSensorNameAsync(
        SensorRenamedEvent sensor,
        CancellationToken cancellationToken);
}