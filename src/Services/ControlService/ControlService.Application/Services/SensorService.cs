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
        Sensor? sensor = await sensorRepository.GetByIdAsync(
            @event.SensorId, cancellationToken);
        if (sensor is null)
        {
            return ConsumerResult.RetryableError(
                $"Sensor {@event.SensorId} not found.");
        }

        sensor.SetState(@event.State);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

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
            @event.CreatedAt);

        return await CreateValidSensorAsync(form, cancellationToken);
    }

    public async Task<ConsumerResult> DeletedSensorAsync(
        SensorDeletedEvent @event,
        CancellationToken cancellationToken)
    {
        Sensor? sensor = await sensorRepository.GetByIdAsync(
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
        Sensor? sensor = await sensorRepository.GetByIdAsync(
            @event.SensorId, cancellationToken);
        if (sensor is null)
        {
            return ConsumerResult.RetryableError(
                $"Sensor {@event.SensorId} not found.");
        }

        Ecosystem? ecosystem = await ecosystemRepository.GetByIdAsync(
            sensor.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return ConsumerResult.RetryableError(
                $"Ecosystem {sensor.EcosystemId} not found.");
        }

        sensor.SetState(SensorState.Faulty);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        IReadOnlyList<AutomationRule>? affectedRules = await ruleRepository.GetBySensorIdWithConditionsAsync(
            sensor.Id, cancellationToken);
        if (affectedRules is null || !affectedRules.Any())
        {
            return ConsumerResult.Success();
        }

        foreach (AutomationRule rule in affectedRules)
        {
            await publishEndpoint.Publish(new ChangeRelayStateEvent
            {
                ControllerId = ecosystem.ControllerId,
                RelayId = rule.RelayId,
                TargetState = false,
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
        Sensor? sensor = await sensorRepository.GetByIdAsync(@event.SensorId, cancellationToken);

        if (sensor is null)
        {
            var form = new SensorForm(
                @event.SensorId,
                @event.ControllerId,
                @event.Name,
                @event.Type,
                @event.State,
                @event.CreatedAt);

            ConsumerResult creationResult = await CreateValidSensorAsync(form, cancellationToken);

            return creationResult;
        }

        sensor.SetName(@event.Name);
        sensor.SetType(@event.Type);
        sensor.SetState(@event.State);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> SetSensorNameAsync(
        SensorRenamedEvent @event,
        CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(
            @event.SensorId, cancellationToken);
        if (existingSensor is null)
        {
            return ConsumerResult.RetryableError(
                $"Sensor {@event.SensorId} not found.");
        }

        existingSensor.SetName(@event.Name);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    private async Task<ConsumerResult> CreateValidSensorAsync(
        SensorForm form,
        CancellationToken cancellationToken)
    {
        Ecosystem? ecosystem = await ecosystemRepository.GetByControllerIdAsync(
            form.ControllerId, cancellationToken);
        if (ecosystem is null)
        {
            return ConsumerResult.RetryableError(
                $"Ecosystem with controller {form.ControllerId} not found.");
        }

        Result<Sensor> result = Sensor.Create(
            form.SensorId,
            form.ControllerId,
            ecosystem.Id,
            form.Name,
            form.State,
            form.Type,
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
        SensorType Type,
        SensorState State,
        DateTime CreatedAt);
}
