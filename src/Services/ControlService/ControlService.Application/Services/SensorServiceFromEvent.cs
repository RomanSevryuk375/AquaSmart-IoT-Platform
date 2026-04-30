using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Events.SensorEvents;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MassTransit;

namespace Control.Application.Services;

public class SensorServiceFromEvent(
    ISensorRepository sensorRepository,
    IAutomationRuleRepository ruleRepository,
    IAquariumRepository aquariumRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork) : ISensorServiceFromEvent
{
    public async Task ChangedStateFromEventAsync(
        SensorStateChangedCommand sensorState, 
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(
            sensorState.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return;
        }

        existingSensor.SetState(sensorState.State);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateSensorFromEventAsync(
        SensorCreatedEvent sensorCreated, 
        CancellationToken cancellationToken)
    {
        if (await sensorRepository
            .GetByIdAsync(sensorCreated.SensorId, cancellationToken) is not null)
        {
            return;
        }

        var existingAquarium = await aquariumRepository
            .GetByControllerIdAsync(sensorCreated.ControllerId, cancellationToken);

        if (existingAquarium is null) 
        {
            return;
        }

        var (sensor, errors) = SensorEntity.Create(
            sensorCreated.SensorId,
            existingAquarium.Id,
            sensorCreated.State,
            sensorCreated.Type,
            sensorCreated.CreatedAt);

        if (errors.Count > 0)
        {
            return;
        }

        await sensorRepository.AddAsync(sensor!, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletedSensorFromEventAsync(
        SensorDeletedEvent sensorDeleted, 
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(
            sensorDeleted.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return;
        }

        await sensorRepository.DeleteAsync(sensorDeleted.SensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleSensorNoDataEvent(
        SensorNoDataEvent sensor,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(
            sensor.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            return;
        }

        var existingAquarium = await aquariumRepository.GetByIdAsync(
            existingSensor.EcosystemId, cancellationToken);

        if (existingAquarium is null)
        {
            return;
        }

        existingSensor.SetState(SensorStateEnum.Faulty);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var affectedRules = await ruleRepository
            .GetBySensorIdAsync(existingSensor.Id, cancellationToken);

        if (affectedRules is null)
        {
            return;
        }

        foreach (var rule in affectedRules)
        {
            await publishEndpoint.Publish(new ChangeRelayStateCommand
            {
                //ControllerId = 
                RelayId = rule.RelayId,
                Action = RuleActionEnum.SwitchOff,
            }, cancellationToken);

            await publishEndpoint.Publish(new SensorNoDataAlertEvent
            {
                UserId = existingAquarium.UserId,
                AquariumId = rule.EcosystemId,
                SensorId = existingSensor.Id,
                LastSeenAt = DateTime.UtcNow,
            }, cancellationToken);
        }
    }

    public async Task UpdatedSensorFromEventAsync(
        SensorUpdatedEvent sensorUpdated, 
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository.GetByIdAsync(
            sensorUpdated.SensorId, cancellationToken);

        if (existingSensor is null)
        {
            var existingAquarium = await aquariumRepository
            .GetByControllerIdAsync(sensorUpdated.ControllerId, cancellationToken);

            if (existingAquarium is null)
            {
                return;
            }

            var (sensor, errors) = SensorEntity.Create(
                sensorUpdated.SensorId,
                existingAquarium.Id,
                sensorUpdated.State,
                sensorUpdated.Type,
                sensorUpdated.CreatedAt);

            if (errors.Count > 0)
            {
                return;
            }

            await sensorRepository.AddAsync(sensor!, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return;
        }

        existingSensor.SetState(sensorUpdated.State);
        existingSensor.SetType(sensorUpdated.Type); 

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
