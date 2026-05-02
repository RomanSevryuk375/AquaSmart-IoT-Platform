using Contracts.Enums;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Services;

public class SensorService(
    ISensorRepository sensorRepository,
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : ISensorService
{
    public async Task<ConsumerResult> CreateSensorAsync(
        SensorCreatedEvent sensor, 
        CancellationToken cancellationToken)
    {
        var exists = await sensorRepository
            .ExistsAsync(sensor.SensorId, cancellationToken);

        if (exists)
        {
            return ConsumerResult.Success();
        }

        var ecosystem = await ecosystemRepository
            .GetByControllerIdAsync(sensor.ControllerId, cancellationToken);

        if (ecosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem for controller {sensor.ControllerId} not found.");
        }

        var (newSensor, errors) = SensorEntity.Create(
            sensor.SensorId,
            sensor.ControllerId,
            ecosystem.Id,
            sensor.Name,
            sensor.Type,
            sensor.State,
            sensor.Unit,
            0.0,
            DateTime.UtcNow,
            sensor.CreatedAt);

        if (newSensor is null)
        {
            return ConsumerResult
                .FatalError($"Failed to create {nameof(SensorEntity)}: " +
                $"{string.Join(", ", errors!)}"); ;
        }

        await sensorRepository.AddAsync(newSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> DeleteSensorAsync(
        SensorDeletedEvent sensor, 
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensor.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return ConsumerResult.Success();
        }

        await sensorRepository.DeleteAsync(existingSensor.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdateSensorAsync(
        SensorUpdatedEvent sensor, 
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensor.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            var ecosystem = await ecosystemRepository
            .GetByControllerIdAsync(sensor.ControllerId, cancellationToken);

            if (ecosystem is null)
            {
                return ConsumerResult
                    .RetryableError($"Ecosystem for controller {sensor.ControllerId} not found.");
            }

            var (newSensor, validationErrors) = SensorEntity.Create(
                sensor.SensorId,
                sensor.ControllerId,
                ecosystem.Id,
                sensor.Name,
                sensor.Type,
                sensor.State,
                sensor.Unit,
                0.0,
                DateTime.UtcNow,
                sensor.CreatedAt);

            if (newSensor is null)
            {
                return ConsumerResult
                    .FatalError($"Failed to create {nameof(SensorEntity)}: " +
                    $"{string.Join(", ", validationErrors!)}");
            }

            await sensorRepository.AddAsync(newSensor, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ConsumerResult.Success();
        }

        var errors = existingSensor!.Update(
            sensor.ControllerId,
            sensor.Type,
            sensor.Unit);

        if (errors is not null && errors.Count > 0)
        {
            return ConsumerResult
                    .FatalError($"Failed to create {nameof(EcosystemEntity)}: " +
                    $"{string.Join(", ", errors!)}");
        }

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> SetSensorStateAsync(
        SensorStateChangedCommand sensor,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensor.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return ConsumerResult
                .FatalError($"Sensor {sensor.SensorId} not found.");
        }

        existingSensor.SetState(sensor.State);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> SetSensorNameAsync(
        SensorRenamedEvent sensor,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensor.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return ConsumerResult
                .RetryableError($"Sensor {sensor.SensorId} not found.");
        }

        existingSensor.Rename(sensor.Name);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
