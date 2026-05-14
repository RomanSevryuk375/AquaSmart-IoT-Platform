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
        SensorStateChangedEvent @event,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(
            @event.SensorId, cancellationToken);

        if (sensor is null)
        {
            return ConsumerResult
                .RetryableError($"Sensor {@event.SensorId} not found. ");
        }

        sensor.SetState(@event.State);

        await sensorRepository.UpdateAsync(sensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> CreateSensorAsync(
        SensorCreatedEvent @event,
        CancellationToken cancellationToken)
    {
        if (await sensorRepository
            .GetByIdAsync(@event.SensorId, cancellationToken) is not null)
        {
            return ConsumerResult.Success();
        }

        var ecosystem = await ecosystemRepository
            .GetByControllerIdAsync(@event.ControllerId, cancellationToken);

        if (ecosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {@event.SensorId} not found. ");
        }

        var result = SensorEntity.Create(
            @event.SensorId,
            @event.ControllerId,
            ecosystem.Id,
            @event.Name,
            @event.State,
            @event.Type,
            @event.CreatedAt);

        if (result.IsFailure)
        {
            return ConsumerResult.FatalError($"{result.Error}");
        }

        await sensorRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> DeletedSensorAsync(
        SensorDeletedEvent @event,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(
            @event.SensorId, cancellationToken);

        if (sensor is null)
        {
            return ConsumerResult.Success();
        }

        await sensorRepository.DeleteAsync(@event.SensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> HandleSensorNoDataEventAsync(
        SensorNoDataEvent @event,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(
            @event.SensorId, cancellationToken);

        if (sensor is null)
        {
            return ConsumerResult
                .RetryableError($"Sensor {@event.SensorId} not found. ");
        }

        var ecosystem = await ecosystemRepository.GetByIdAsync(
            sensor.EcosystemId, cancellationToken);

        if (ecosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {sensor.EcosystemId} not found. ");
        }

        sensor.SetState(SensorStateEnum.Faulty);

        await sensorRepository.UpdateAsync(sensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var affectedRules = await ruleRepository
            .GetBySensorIdWithConditionsAsync(sensor.Id, cancellationToken);

        if (affectedRules is null)
        {
            return ConsumerResult.Success();
        }

        foreach (var rule in affectedRules)
        {
            await publishEndpoint.Publish(new ChangeRelayStateEvent
            {
                ControllerId = ecosystem.ControllerId,
                RelayId = rule.RelayId,
                Action = RuleActionEnum.SwitchOff,
                ExpireAt = DateTime.UtcNow.AddMinutes(5)
            }, cancellationToken);

            await publishEndpoint.Publish(new SensorNoDataAlertEvent
            {
                UserId = ecosystem.UserId,
                EcosytemId = rule.EcosystemId,
                SensorId = sensor.Id,
                LastSeenAt = DateTime.UtcNow,
            }, cancellationToken);
        }

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdatedSensorAsync(
        SensorUpdatedEvent @event,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(
            @event.SensorId, cancellationToken);

        if (sensor is null)
        {
            var ecosystem = await ecosystemRepository
            .GetByControllerIdAsync(@event.ControllerId, cancellationToken);

            if (ecosystem is null)
            {
                return ConsumerResult
                    .RetryableError($"Ecosystem with controller{@event.ControllerId} not found. ");
            }

            var result = SensorEntity.Create(
                @event.SensorId,
                @event.ControllerId,
                ecosystem.Id,
                @event.Name,
                @event.State,
                @event.Type,
                @event.CreatedAt);

            if (result.IsFailure)
            {
                return ConsumerResult.FatalError($"{result.Error}");
            }

            await sensorRepository.AddAsync(result.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ConsumerResult.Success();
        }

        sensor.SetState(@event.State);
        sensor.SetType(@event.Type);

        await sensorRepository.UpdateAsync(sensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
