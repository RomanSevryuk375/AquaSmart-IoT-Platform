using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
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
        Sensor? triggerSensor = await sensorRepository.GetByIdAsync(
            telemetry.SensorId, cancellationToken);
        if (triggerSensor is null)
        {
            return ConsumerResult.FatalError(
                $"Trigger sensor {telemetry.SensorId} not found in ControlService.");
        }

        triggerSensor.SetLastValue(telemetry.Value);

        IReadOnlyList<AutomationRule> rules = await ruleRepository.GetBySensorIdWithConditionsAsync(
            telemetry.SensorId, cancellationToken);

        if (rules.Count == 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ConsumerResult.Success();
        }

        Dictionary<Guid, double> sensorValuesCache = await LoadSensorValuesAsync(
            rules, telemetry, cancellationToken);

        Dictionary<Guid, Relay> relaysCache = await LoadRequiredRelaysAsync(
            rules, cancellationToken);

        foreach (AutomationRule rule in rules)
        {
            bool? targetState = rule.EvaluateTargetState(sensorValuesCache);

            if (targetState.HasValue && relaysCache.TryGetValue(rule.RelayId, out Relay? relay))
            {
                ApplyRelayState(relay, targetState.Value);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    private async Task<Dictionary<Guid, double>> LoadSensorValuesAsync(
        IEnumerable<AutomationRule> rules,
        TelemetryReceivedEvent telemetry,
        CancellationToken cancellationToken)
    {
        var neededSensorIds = rules
            .SelectMany(r => r.Conditions)
            .Select(c => c.SensorId)
            .Distinct()
            .ToList();

        IReadOnlyList<Sensor> sensors = await sensorRepository.GetManyByIdsAsync(
            neededSensorIds, cancellationToken);

        var valuesCache = new Dictionary<Guid, double>();
        foreach (Sensor sensor in sensors)
        {
            valuesCache[sensor.Id] = sensor.Id == telemetry.SensorId
                ? telemetry.Value
                : sensor.LastValue;
        }

        return valuesCache;
    }

    private async Task<Dictionary<Guid, Relay>> LoadRequiredRelaysAsync(
        IEnumerable<AutomationRule> rules,
        CancellationToken cancellationToken)
    {
        var neededRelayIds = rules
            .Select(r => r.RelayId)
            .Distinct()
            .ToList();

        IReadOnlyList<Relay> relays = await relayRepository.GetManyByIds(
            neededRelayIds, cancellationToken);

        return relays.ToDictionary(r => r.Id);
    }

    private static void ApplyRelayState(Relay relay, bool targetState)
    {
        if (relay.IsManual || relay.IsActive == targetState)
        {
            return;
        }

        DateTime actionExpireAt = DateTime.UtcNow.AddMinutes(5);
        relay.SetState(targetState, actionExpireAt);
    }
}
