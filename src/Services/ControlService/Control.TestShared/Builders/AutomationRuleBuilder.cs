using Control.TestShared.Constants;

namespace Control.TestShared.Builders;

public class AutomationRuleBuilder
{
    private Guid _id = ControlTestConstants.AutomationRuleId;
    private Guid _ecosystemId = ControlTestConstants.EcosystemId;
    private string _name = "Test Automation Rule";
    private Guid _relayId = ControlTestConstants.RelayId;
    private Operator _operator = Operator.AND;
    private RuleAction _action = RuleAction.SwitchOn;
    private bool _isActive = true;
    private readonly List<RuleCondition> _conditions = [];

    public AutomationRuleBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public AutomationRuleBuilder WithEcosystemId(Guid ecosystemId)
    {
        _ecosystemId = ecosystemId;
        return this;
    }

    public AutomationRuleBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public AutomationRuleBuilder WithRelayId(Guid relayId)
    {
        _relayId = relayId;
        return this;
    }

    public AutomationRuleBuilder WithOperator(Operator @operator)
    {
        _operator = @operator;
        return this;
    }

    public AutomationRuleBuilder WithAction(RuleAction action)
    {
        _action = action;
        return this;
    }

    public AutomationRuleBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public AutomationRuleBuilder WithCondition(RuleCondition condition)
    {
        _conditions.Add(condition);
        return this;
    }

    public AutomationRuleBuilder WithConditions(IEnumerable<RuleCondition> conditions)
    {
        _conditions.AddRange(conditions);
        return this;
    }

    public AutomationRule Build()
    {
        Result<AutomationRule> result = AutomationRule.Create(
            _id,
            _ecosystemId,
            _name,
            _relayId,
            _operator,
            _action,
            _isActive);

        if (result.IsFailure)
        {
            throw new ArgumentException($"AutomationRuleBuilder failed: {result.Error.Message}");
        }

        AutomationRule rule = result.Value;
        foreach (RuleCondition condition in _conditions)
        {
            rule.AddCondition(condition);
        }

        rule.ClearDomainEvents();
        return rule;
    }
}
