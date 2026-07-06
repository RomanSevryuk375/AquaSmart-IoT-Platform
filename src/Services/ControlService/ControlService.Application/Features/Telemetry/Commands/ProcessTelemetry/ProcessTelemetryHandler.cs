using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Telemetry.Commands.ProcessTelemetry;

public sealed class ProcessTelemetryHandler(
    IAutomationRuleRepository ruleRepository,
    ISensorRepository sensorRepository,
    IRelayRepository relayRepository) : IRequestHandler<ProcessTelemetryCommand, Result>
{
    public async Task<Result> Handle(ProcessTelemetryCommand request, CancellationToken cancellationToken)
    {
        Sensor? triggerSensor = await sensorRepository.GetByIdAsync(
            request.SensorId, cancellationToken);
        if (triggerSensor is null)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"Trigger sensor {request.SensorId} not found in ControlService."));
        }

        triggerSensor.SetLastValue(request.Value);

        IReadOnlyList<AutomationRule> rules = await ruleRepository.GetBySensorIdWithConditionsAsync(
            request.SensorId, cancellationToken);

        if (rules.Count == 0)
        {
            return Result.Success();
        }

        Dictionary<Guid, double> sensorValuesCache = await LoadSensorValuesAsync(
            rules, request, cancellationToken);

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

        return Result.Success();
    }

    private async Task<Dictionary<Guid, double>> LoadSensorValuesAsync(
        IEnumerable<AutomationRule> rules,
        ProcessTelemetryCommand telemetry,
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
