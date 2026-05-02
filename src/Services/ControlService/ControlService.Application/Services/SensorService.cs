using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MassTransit;

namespace Control.Application.Services;

public sealed class SensorService(
    ISensorRepository sensorRepository,
    IAutomationRuleRepository ruleRepository,
    IEcosystemRepository ecosystemRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork) : ISensorService
{
    public async Task<ConsumerResult> ChangedStateAsync(
        SensorStateChangedCommand sensor,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(
            sensor.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return ConsumerResult
                .RetryableError($"Sensor {sensor.SensorId} not found. ");
        }

        existingSensor.SetState(sensor.State);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> CreateSensorAsync(
        SensorCreatedEvent newSensor,
        CancellationToken cancellationToken)
    {
        if (await sensorRepository
            .GetByIdAsync(newSensor.SensorId, cancellationToken) is not null)
        {
            return ConsumerResult.Success();
        }

        var existingEcosystem = await ecosystemRepository
            .GetByControllerIdAsync(newSensor.ControllerId, cancellationToken);

        if (existingEcosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {newSensor.SensorId} not found. ");
        }

        var (sensor, errors) = SensorEntity.Create(
            newSensor.SensorId,
            newSensor.ControllerId,
            existingEcosystem.Id,
            newSensor.Name,
            newSensor.State,
            newSensor.Type,
            newSensor.CreatedAt);

        if (errors.Count > 0)
        {
            return ConsumerResult
                .FatalError($"Failed to create {nameof(SensorEntity)}: {string.Join(", ", errors)}");
        }

        await sensorRepository.AddAsync(sensor!, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> DeletedSensorAsync(
        SensorDeletedEvent sensor,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(
            sensor.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return ConsumerResult.Success();
        }

        await sensorRepository.DeleteAsync(sensor.SensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> HandleSensorNoDataEventAsync(
        SensorNoDataEvent sensor,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(
            sensor.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return ConsumerResult
                .RetryableError($"Sensor {sensor.SensorId} not found. ");
        }

        var existingEcosystem = await ecosystemRepository.GetByIdAsync(
            existingSensor.EcosystemId, cancellationToken);

        if (existingEcosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {existingSensor.EcosystemId} not found. ");
        }

        existingSensor.SetState(SensorStateEnum.Faulty);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var affectedRules = await ruleRepository
            .GetBySensorIdWithConditionsAsync(existingSensor.Id, cancellationToken);

        if (affectedRules is null)
        {
            return ConsumerResult.Success();
        }

        foreach (var rule in affectedRules)
        {
            await publishEndpoint.Publish(new ChangeRelayStateCommand
            {
                ControllerId = existingEcosystem.ControllerId,
                RelayId = rule.RelayId,
                Action = RuleActionEnum.SwitchOff,
                ExpireAt = DateTime.UtcNow.AddMinutes(5)
            }, cancellationToken);

            await publishEndpoint.Publish(new SensorNoDataAlertEvent
            {
                UserId = existingEcosystem.UserId,
                EcosytemId = rule.EcosystemId,
                SensorId = existingSensor.Id,
                LastSeenAt = DateTime.UtcNow,
            }, cancellationToken);
        }

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdatedSensorAsync(
        SensorUpdatedEvent sensorUpdated,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(
            sensorUpdated.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            var existingEcosystem = await ecosystemRepository
            .GetByControllerIdAsync(sensorUpdated.ControllerId, cancellationToken);

            if (existingEcosystem is null)
            {
                return ConsumerResult
                    .RetryableError($"Ecosystem with controller{sensorUpdated.ControllerId} not found. ");
            }

            var (sensor, errors) = SensorEntity.Create(
                sensorUpdated.SensorId,
                sensorUpdated.ControllerId,
                existingEcosystem.Id,
                sensorUpdated.Name,
                sensorUpdated.State,
                sensorUpdated.Type,
                sensorUpdated.CreatedAt);

            if (errors.Count > 0)
            {
                return ConsumerResult
                    .FatalError($"Failed to create {nameof(SensorEntity)}: {string.Join(", ", errors)}");
            }

            await sensorRepository.AddAsync(sensor!, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ConsumerResult.Success();
        }

        existingSensor.SetState(sensorUpdated.State);
        existingSensor.SetType(sensorUpdated.Type);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
