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
        SensorEntity? triggerSensor = await sensorRepository.GetByIdAsync(
            telemetry.SensorId, cancellationToken);
        if (triggerSensor is null)
        {
            return ConsumerResult.FatalError(
                $"Trigger sensor {telemetry.SensorId} not found in ControlService.");
        }

        triggerSensor.SetLastValue(telemetry.Value);

        IReadOnlyList<AutomationRule> rules = await ruleRepository.GetBySensorIdWithConditionsAsync(
            telemetry.SensorId, cancellationToken);
        if (rules == null || !rules.Any())
        {
            return ConsumerResult.Success();
        }

        Dictionary<Guid, SensorEntity> sensorsCache = await LoadRequiredSensorsAsync(rules, cancellationToken);

        foreach (AutomationRule rule in rules)
        {
            List<bool?> matchConditions = EvaluateConditions(
                rule.Conditions, sensorsCache, telemetry);

            bool? targetState = RuleOperatorFactory.CalculateTargetState(
                matchConditions, rule.Operator, rule.Action);

            if (targetState.HasValue)
            {
                await ApplyRelayStateAsync(rule.RelayId, targetState.Value, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    private async Task<Dictionary<Guid, SensorEntity>> LoadRequiredSensorsAsync(
        IEnumerable<AutomationRule> rules,
        CancellationToken cancellationToken)
    {
        var neededSensorIds = rules
            .SelectMany(r => r.Conditions)
            .Select(c => c.SensorId)
            .Distinct()
            .ToList();

        IReadOnlyList<SensorEntity> sensors = await sensorRepository.GetManyByIdsAsync(neededSensorIds, cancellationToken);
        return sensors.ToDictionary(s => s.Id);
    }

    private static List<bool?> EvaluateConditions(
        ICollection<RuleConditionEntity> conditions,
        Dictionary<Guid, SensorEntity> sensorsCache,
        TelemetryReceivedEvent telemetry)
    {
        var matchConditions = new List<bool?>();

        foreach (RuleConditionEntity condition in conditions)
        {
            if (!sensorsCache.TryGetValue(condition.SensorId, out SensorEntity? sensor))
            {
                continue;
            }

            double valueToEvaluate = condition.SensorId == telemetry.SensorId
                ? telemetry.Value
                : sensor.LastValue;

            IRuleEvaluator evaluator = RuleEvaluatorFactory.Create(condition.Condition);
            bool? isMet = evaluator.Evaluate(valueToEvaluate, condition.Threshold, condition.Hysteresis);

            matchConditions.Add(isMet);
        }

        return matchConditions;
    }

    private async Task ApplyRelayStateAsync(
        Guid relayId,
        bool targetState,
        CancellationToken cancellationToken)
    {
        RelayEntity? relay = await relayRepository.GetByIdAsync(relayId, cancellationToken);

        if (relay is null || relay.IsManual || relay.IsActive == targetState)
        {
            return;
        }

        DateTime actionExpireAt = DateTime.UtcNow.AddMinutes(5);

        relay.SetState(targetState, actionExpireAt);
    }
}
