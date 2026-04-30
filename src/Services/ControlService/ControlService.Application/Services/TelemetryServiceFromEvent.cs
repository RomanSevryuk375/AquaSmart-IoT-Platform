using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Events.TelemetryEvents;
using Control.Application.Interfaces;
using Control.Domain.Factories;
using Control.Domain.Interfaces;
using MassTransit;

namespace Control.Application.Services;

public class TelemetryServiceFromEvent(
    IAutomationRuleRepository ruleRepository,
    IRelayRepository relayRepository,
    IAquariumRepository aquariumRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : ITelemetryServiceFromEvent
{
    public async Task ProcessTelemetryAsync(
        TelemetryReceivedEvent telemetry,
        CancellationToken cancellationToken)
    {
        var rules = await ruleRepository
            .GetBySensorIdAsync(telemetry.SensorId, cancellationToken);

        if (rules == null)
        {
            return;
        }

        foreach (var rule in rules)
        {
            var relay = await relayRepository
                .GetByIdAsync(rule.RelayId, cancellationToken);

            if (relay == null || relay.IsManual)
            {
                continue;
            }

            var aquarium = await aquariumRepository
                .GetByIdAsync(relay.EcosystemId, cancellationToken);

            if (aquarium is null)
            {
                continue;
            }

            var evaluator = RuleEvaluatorFactory.Create(rule.Condition);

            var isMet = evaluator.Evaluate(telemetry.Value, rule.Threshold, rule.Hysteresis);

            if (isMet is null)
            {
                continue;
            }

            bool targetState = isMet.Value
                ? rule.Action == RuleActionEnum.SwitchOn
                : rule.Action == RuleActionEnum.SwitchOff;

            if (isMet is true && Math.Abs(telemetry.Value - rule.Threshold) > 5.0)
            {
                await publishEndpoint.Publish(new CriticalTelemetryThresholdAlertEvent
                {
                    UserId = aquarium.UserId,
                    AquariumId = rule.EcosystemId,
                    SensorId = telemetry.SensorId,
                    Value = telemetry.Value,
                    RecordedAt = telemetry.RecordedAt,
                    RelayId = rule.RelayId,
                    RelayState = targetState
                }, cancellationToken);
            }

            if (relay.IsActive == targetState)
            {
                continue;
            }

            relay.SetState(targetState);

            await relayRepository.UpdateAsync(relay, cancellationToken);

            await publishEndpoint.Publish(new ChangeRelayStateCommand
            {
                RelayId = relay.Id,
                Action = StateEvaluatorFactory.EvaluateBool(targetState),
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
