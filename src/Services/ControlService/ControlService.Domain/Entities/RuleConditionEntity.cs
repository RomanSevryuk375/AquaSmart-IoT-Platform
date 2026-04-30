using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Domain.Entities;

public sealed class RuleConditionEntity : IEntity
{
    private RuleConditionEntity(
        Guid id,
        Guid automationRuleId,
        Guid sensorId,
        RuleConditionEnum condition,
        double threshold,
        double hysteresis,
        DateTime createdAt)
    {
        Id = id;
        AutomationRuleId = automationRuleId;
        SensorId = sensorId;
        Condition = condition;
        Threshold = threshold;
        Hysteresis = hysteresis;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid AutomationRuleId { get; private set; }
    public Guid SensorId { get; private set; }
    public RuleConditionEnum Condition { get; private set; }
    public double Threshold { get; private set; }
    public double Hysteresis { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static (RuleConditionEntity? ruleCondition, List<string> errors) Create(
        Guid automationRuleId,
        Guid sensorId,
        RuleConditionEnum condition,
        double threshold,
        double hysteresis)
    {
        var errors = new List<string>();

        if (sensorId == Guid.Empty)
        {
            errors.Add("sensorId is required.");
        }

        if (hysteresis < 0)
        {
            errors.Add("hysteresis cannot be negative.");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var ruleCondition = new RuleConditionEntity(
            Guid.NewGuid(),
            automationRuleId,
            sensorId,
            condition,
            threshold,
            hysteresis,
            DateTime.UtcNow);

        return (ruleCondition, errors);
    }

    public List<string>? Update(
        RuleConditionEnum condition,
        double threshold,
        double hysteresis)
    {
        var errors = new List<string>();

        if (hysteresis < 0)
        {
            errors.Add("hysteresis must not be less then zero.");
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Condition = condition;
        Threshold = threshold;
        Hysteresis = hysteresis;

        return null;
    }

    public void SetCondition(RuleConditionEnum condition)
    {
        if (Condition == condition)
        {
            return;
        }

        Condition = condition;
    }

    public void SetThreshold(double threshold)
    {
        Threshold = threshold;
    }

    public string? SetHysteresis(double hysteresis)
    {
        if (hysteresis < 0)
        {
            return ("hysteresis must not be less then zero.");
        }

        Hysteresis = hysteresis;

        return null;
    }
}