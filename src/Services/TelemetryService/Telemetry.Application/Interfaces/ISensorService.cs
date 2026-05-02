using Contracts.Events.SensorEvents;
using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface ISensorService
{
    Task<ConsumerResult> CreateSensorAsync(
        SensorCreatedEvent sensorCreated, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> DeleteSensorAsync(
        SensorDeletedEvent sensorDeleted, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> UpdateSensorAsync(
        SensorUpdatedEvent updatedEvent, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> SetSensorStateAsync(
        SensorStateChangedCommand sensorStateChanged,
        CancellationToken cancellationToken);

    Task<ConsumerResult> SetSensorNameAsync(
        SensorRenamedEvent sensor,
        CancellationToken cancellationToken);
}