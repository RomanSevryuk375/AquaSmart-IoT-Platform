using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Control.Domain.Factories;
using Control.Domain.Interfaces;
using Control.Domain.ValueObjects;

namespace Control.Domain.Entities;

public sealed class RuleCondition : IEntity
{
    private RuleCondition(
        Guid id,
        Guid automationRuleId,
        Guid sensorId,
        Condition conditionType,
        ConditionThreshold conditionThreshold,
        DateTime createdAt)
    {
        Id = id;
        AutomationRuleId = automationRuleId;
        SensorId = sensorId;
        Condition = conditionType;
        ConditionThreshold = conditionThreshold;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private RuleCondition() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid AutomationRuleId { get; private set; }
    public Guid SensorId { get; private set; }
    public Condition Condition { get; private set; }
    public ConditionThreshold ConditionThreshold { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<RuleCondition> Create(
        Guid ruleConditionId,
        Guid automationRuleId,
        Guid sensorId,
        Condition condition,
        double threshold,
        double hysteresis)
    {
        Result<ConditionThreshold> conditionThresholdResult = ConditionThreshold.Create(
            threshold, hysteresis);
        if (conditionThresholdResult.IsFailure)
        {
            return Result<RuleCondition>.Failure(conditionThresholdResult.Error);
        }

        var ruleCondition = new RuleCondition(
            ruleConditionId, automationRuleId, sensorId,
            condition, conditionThresholdResult.Value,
            createdAt: DateTime.UtcNow);

        return Result<RuleCondition>.Success(ruleCondition);
    }

    public Result Update(
        Condition condition,
        double threshold,
        double hysteresis)
    {
        Result<ConditionThreshold> conditionThresholdResult = ConditionThreshold.Create(
            threshold, hysteresis);
        if (conditionThresholdResult.IsFailure)
        {
            return Result.Failure(conditionThresholdResult.Error);
        }

        Condition = condition;
        ConditionThreshold = conditionThresholdResult.Value;

        return Result.Success();
    }

    public void SetConditionType(Condition condition)
    {
        if (Condition == condition)
        {
            return;
        }

        Condition = condition;
    }

    public bool? Evaluate(double valueToEvaluate)
    {
        IRuleEvaluator evaluator = RuleEvaluatorFactory.Create(Condition);
        bool? isMet = evaluator.Evaluate(
            valueToEvaluate,
            ConditionThreshold.Threshold,
            ConditionThreshold.Hysteresis);

        return isMet;
    }
}
