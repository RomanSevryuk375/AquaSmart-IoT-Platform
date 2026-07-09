using Control.TestShared.Constants;

namespace Control.TestShared.Builders;

public class RuleConditionBuilder
{
    private Guid _id = ControlTestConstants.RuleConditionId;
    private Guid _automationRuleId = ControlTestConstants.AutomationRuleId;
    private Guid _sensorId = ControlTestConstants.SensorId;
    private Condition _condition = Condition.Greater;
    private double _threshold = 25.0;
    private double _hysteresis = 0.5;

    public RuleConditionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RuleConditionBuilder WithAutomationRuleId(Guid automationRuleId)
    {
        _automationRuleId = automationRuleId;
        return this;
    }

    public RuleConditionBuilder WithSensorId(Guid sensorId)
    {
        _sensorId = sensorId;
        return this;
    }

    public RuleConditionBuilder WithCondition(Condition condition)
    {
        _condition = condition;
        return this;
    }

    public RuleConditionBuilder WithThreshold(double threshold)
    {
        _threshold = threshold;
        return this;
    }

    public RuleConditionBuilder WithHysteresis(double hysteresis)
    {
        _hysteresis = hysteresis;
        return this;
    }

    public RuleCondition Build()
    {
        Result<RuleCondition> result = RuleCondition.Create(
            _id,
            _automationRuleId,
            _sensorId,
            _condition,
            _threshold,
            _hysteresis);

        if (result.IsFailure)
        {
            throw new ArgumentException($"RuleConditionBuilder failed: {result.Error.Message}");
        }

        return result.Value;
    }
}
