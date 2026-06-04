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
        SensorCreatedEvent @event,
        CancellationToken cancellationToken)
    {
        if (await sensorRepository.GetByIdAsync(@event.SensorId, cancellationToken) is not null)
        {
            return ConsumerResult.Success();
        }

        var form = new SensorForm(
            @event.SensorId,
            @event.ControllerId,
            @event.Name,
            @event.Type,
            @event.State,
            @event.Unit,
            @event.CreatedAt);

        return await CreateValidSensorAsync(form, cancellationToken);
    }

    public async Task<ConsumerResult> DeletedSensorAsync(
        SensorDeletedEvent @event,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(@event.SensorId, cancellationToken);
        if (sensor is null)
        {
            return ConsumerResult.Success();
        }

        await sensorRepository.DeleteAsync(@event.SensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdatedSensorAsync(
        SensorUpdatedEvent @event,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(@event.SensorId, cancellationToken);
        if (sensor is null)
        {
            var form = new SensorForm(
                @event.SensorId,
                @event.ControllerId,
                @event.Name,
                @event.Type,
                @event.State,
                @event.Unit,
                @event.CreatedAt);

            var creationResult = await CreateValidSensorAsync(form, cancellationToken);

            return creationResult;
        }

        sensor.SetName(@event.Name);
        sensor.SetType(@event.Type);
        sensor.SetState(@event.State);

        await sensorRepository.UpdateAsync(sensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> SetSensorStateAsync(
        SensorStateChangedEvent @event,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(@event.SensorId, cancellationToken);
        if (sensor is null)
        {
            return ConsumerResult.RetryableError($"Sensor {@event.SensorId} not found.");
        }

        sensor.SetState(@event.State);

        await sensorRepository.UpdateAsync(sensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> SetSensorNameAsync(
        SensorRenamedEvent sensor,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(sensor.SensorId, cancellationToken);
        if (existingSensor is null)
        {
            return ConsumerResult
                .RetryableError($"Sensor {sensor.SensorId} not found.");
        }

        existingSensor.SetName(sensor.Name);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    private async Task<ConsumerResult> CreateValidSensorAsync(
        SensorForm form,
        CancellationToken cancellationToken)
    {
        var ecosystem = await ecosystemRepository.GetByControllerIdAsync(form.ControllerId, cancellationToken);
        if (ecosystem is null)
        {
            return ConsumerResult.RetryableError(
                $"Ecosystem with controller {form.ControllerId} not found.");
        }

        var result = SensorEntity.Create(
            form.SensorId,
            form.ControllerId,
            ecosystem.Id,
            form.Name,
            form.Type,
            form.State,
            form.Unit,
            0.0,
            DateTime.UtcNow,
            form.CreatedAt);

        if (result.IsFailure)
        {
            return ConsumerResult.FatalError($"{result.Error}");
        }

        await sensorRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    private record SensorForm(
        Guid SensorId,
        Guid ControllerId,
        string Name,
        SensorTypeEnum Type,
        SensorStateEnum State,
        string Unit,
        DateTime CreatedAt);
}
