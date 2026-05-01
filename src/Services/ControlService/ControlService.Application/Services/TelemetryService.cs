using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Factories;
using Control.Domain.Interfaces;
using MassTransit;

namespace Control.Application.Services;

public sealed class TelemetryService(
    IAutomationRuleRepository ruleRepository,
    ISensorRepository sensorRepository,
    IRelayRepository relayRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : ITelemetryService
{
    public async Task<ConsumerResult> ProcessTelemetryAsync(
        TelemetryReceivedEvent telemetry,
        CancellationToken cancellationToken)
    {
        var triggerSensor = await sensorRepository
            .GetByIdAsync(telemetry.SensorId, cancellationToken);

        if (triggerSensor is null)
        {
            return ConsumerResult
                .FatalError($"Trigger sensor {telemetry.SensorId} not found in ControlService.");
        }
        triggerSensor.SetLastValue(telemetry.Value);
        await sensorRepository.UpdateAsync(triggerSensor, cancellationToken);

        var rules = await ruleRepository
            .GetBySensorIdWithConditionsAsync(telemetry.SensorId, cancellationToken);

        if (rules == null || !rules.Any())
        {
            return ConsumerResult.Success();
        }

        var allNeededSensorIds = rules
            .SelectMany(r => r.Conditions)
            .Select(c => c.SensorId)
            .Distinct()
            .ToList();

        var sensorsCache = (await sensorRepository
            .GetManyByIdsAsync(allNeededSensorIds, cancellationToken))
            .ToDictionary(s => s.Id);

        foreach (var rule in rules)
        {
            var matchConditions = new List<bool?>();

            foreach (var condition in rule.Conditions)
            {
                if (!sensorsCache.TryGetValue(condition.SensorId, out var sensor))
                {
                    continue;
                }

                var valueToEvaluate = (condition.SensorId == telemetry.SensorId)
                    ? telemetry.Value
                    : sensor.LastValue;

                var evaluator = RuleEvaluatorFactory.Create(condition.Condition);
                var isMet = evaluator.Evaluate(valueToEvaluate, condition.Threshold, condition.Hysteresis);
                matchConditions.Add(isMet);
            }

            var targetState = RuleOperatorFactory.CalculateTargetState(matchConditions, rule.Operator, rule.Action);

            if (!targetState.HasValue)
            {
                continue;
            }

            var relay = await relayRepository
                .GetByIdAsync(rule.RelayId, cancellationToken);

            if (relay is null ||
                relay.IsManual ||
                relay.IsActive == targetState.Value)
            {
                continue;
            }

            relay.SetState(targetState.Value);
            await relayRepository.UpdateAsync(relay, cancellationToken);

            await publishEndpoint.Publish(new ChangeRelayStateCommand
            {
                ControllerId = relay.ControllerId,
                RelayId = relay.Id,
                Action = StateEvaluatorFactory.EvaluateBool(targetState.Value),
                ExpireAt = DateTime.UtcNow.AddMinutes(5)
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}