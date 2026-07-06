using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace Control.Application.Features.Sensors.Commands.HandleSensorNoData;

public sealed class HandleSensorNoDataHandler(
    ISensorRepository sensorRepository,
    IEcosystemRepository ecosystemRepository,
    IAutomationRuleRepository ruleRepository,
    IPublishEndpoint publishEndpoint) : IRequestHandler<HandleSensorNoDataCommand, Result>
{
    public async Task<Result> Handle(HandleSensorNoDataCommand request, CancellationToken cancellationToken)
    {
        Sensor? sensor = await sensorRepository.GetByIdAsync(
            request.SensorId, cancellationToken);
        if (sensor is null)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"Sensor {request.SensorId} not found."));
        }

        Ecosystem? ecosystem = await ecosystemRepository.GetByIdAsync(
            sensor.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem {sensor.EcosystemId} not found."));
        }

        sensor.SetState(SensorState.Faulty);

        IReadOnlyList<AutomationRule> affectedRules = await ruleRepository.GetBySensorIdWithConditionsAsync(
            sensor.Id, cancellationToken);
        if (affectedRules.Count > 0)
        {
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
                    LastSeenAt = request.LastSeenAt
                }, cancellationToken);
            }
        }

        return Result.Success();
    }
}
