using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
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
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(ruleConditionId, "RuleCondition Id", errors);
        DomainGuard.CheckNotEmpty(automationRuleId, nameof(AutomationRuleId), errors);
        DomainGuard.CheckNotEmpty(sensorId, nameof(SensorId), errors);
        DomainGuard.CheckEnum(condition, nameof(Condition), errors);
        ConditionThreshold? conditionThreshold = DomainGuard.Collect(
            ConditionThreshold.Create(threshold, hysteresis), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<RuleCondition>(errors);
        }

        var ruleCondition = new RuleCondition(
            ruleConditionId, automationRuleId, sensorId,
            condition, conditionThreshold!,
            createdAt: DateTime.UtcNow);

        return Result<RuleCondition>.Success(ruleCondition);
    }

    public Result Update(
        Condition condition,
        double threshold,
        double hysteresis)
    {
        var errors = new List<string>();

        DomainGuard.CheckEnum(condition, nameof(Condition), errors);
        ConditionThreshold? conditionThreshold = DomainGuard.Collect(
            ConditionThreshold.Create(threshold, hysteresis), errors);

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation<RuleCondition>(string.Join(", ", errors)));
        }

        Condition = condition;
        ConditionThreshold = conditionThreshold!;

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
