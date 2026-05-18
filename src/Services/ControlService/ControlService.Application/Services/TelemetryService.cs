using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Factories;
using Control.Domain.Interfaces;

namespace Control.Application.Services;

public sealed class TelemetryService(
    IAutomationRuleRepository ruleRepository,
    ISensorRepository sensorRepository,
    IRelayRepository relayRepository,
    IUnitOfWork unitOfWork) : ITelemetryService
{
    public async Task<ConsumerResult> ProcessTelemetryAsync(
        TelemetryReceivedEvent telemetry, 
        CancellationToken cancellationToken)
    {
        var triggerSensor = await sensorRepository.GetByIdAsync(
            telemetry.SensorId, cancellationToken);
        if (triggerSensor is null)
        {
            return ConsumerResult.FatalError(
                $"Trigger sensor {telemetry.SensorId} not found in ControlService.");
        }

        triggerSensor.SetLastValue(telemetry.Value);
        await sensorRepository.UpdateAsync(triggerSensor, cancellationToken);

        var rules = await ruleRepository.GetBySensorIdWithConditionsAsync(
            telemetry.SensorId, cancellationToken);
        if (rules == null || !rules.Any())
        {
            return ConsumerResult.Success();
        }

        var sensorsCache = await LoadRequiredSensorsAsync(rules, cancellationToken);

        foreach (var rule in rules)
        {
            var matchConditions = EvaluateConditions(rule.Conditions, sensorsCache, telemetry);

            var targetState = RuleOperatorFactory.CalculateTargetState(matchConditions, rule.Operator, rule.Action);

            if (targetState.HasValue)
            {
                await ApplyRelayStateAsync(rule.RelayId, targetState.Value, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    private async Task<Dictionary<Guid, SensorEntity>> LoadRequiredSensorsAsync(
        IEnumerable<AutomationRuleEntity> rules,
        CancellationToken cancellationToken)
    {
        var neededSensorIds = rules
            .SelectMany(r => r.Conditions)
            .Select(c => c.SensorId)
            .Distinct()
            .ToList();

        var sensors = await sensorRepository.GetManyByIdsAsync(neededSensorIds, cancellationToken);
        return sensors.ToDictionary(s => s.Id);
    }

    private static List<bool?> EvaluateConditions(
        ICollection<RuleConditionEntity> conditions,
        Dictionary<Guid, SensorEntity> sensorsCache,
        TelemetryReceivedEvent telemetry)
    {
        var matchConditions = new List<bool?>();

        foreach (var condition in conditions)
        {
            if (!sensorsCache.TryGetValue(condition.SensorId, out var sensor))
            {
                continue;
            }

            var valueToEvaluate = condition.SensorId == telemetry.SensorId
                ? telemetry.Value
                : sensor.LastValue;

            var evaluator = RuleEvaluatorFactory.Create(condition.Condition);
            var isMet = evaluator.Evaluate(valueToEvaluate, condition.Threshold, condition.Hysteresis);

            matchConditions.Add(isMet);
        }

        return matchConditions; 
    }

    private async Task ApplyRelayStateAsync(
        Guid relayId,
        bool targetState,
        CancellationToken cancellationToken)
    {
        var relay = await relayRepository.GetByIdAsync(relayId, cancellationToken);

        if (relay is null || relay.IsManual || relay.IsActive == targetState)
        {
            return; 
        }

        var actionExpireAt = DateTime.UtcNow.AddMinutes(5); 

        relay.SetState(targetState, actionExpireAt);

        await relayRepository.UpdateAsync(relay, cancellationToken);
    }
}