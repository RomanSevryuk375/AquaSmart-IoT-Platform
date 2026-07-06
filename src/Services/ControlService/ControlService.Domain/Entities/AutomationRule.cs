using Contracts.Abstractions;
using Contracts.Constants;
using Contracts.Enums;
using Contracts.Results;
using Control.Domain.Factories;
using Control.Domain.ValueObjects;

namespace Control.Domain.Entities;

public sealed class AutomationRule : AggregateRoot, IEntity
{
    private AutomationRule(
        Guid id,
        Guid ecosystemId,
        Guid relayId,
        Name name,
        Operator @operator,
        RuleAction action,
        bool isActive,
        DateTime createdAt)
    {
        Id = id;
        EcosystemId = ecosystemId;
        RelayId = relayId;
        Name = name;
        Operator = @operator;
        Action = action;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private AutomationRule() { }
#pragma warning restore CS8618

    private readonly List<RuleCondition> _ruleConditions = [];

    public Guid Id { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Guid RelayId { get; private set; }
    public Name Name { get; private set; }
    public Operator Operator { get; private set; }
    public RuleAction Action { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<RuleCondition> Conditions => _ruleConditions.AsReadOnly();

    public static Result<AutomationRule> Create(
        Guid ruleId,
        Guid ecosystemId,
        string rawName,
        Guid relayId,
        Operator @operator,
        RuleAction action,
        bool isActive)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result<AutomationRule>.Failure(nameResult.Error);
        }

        var rule = new AutomationRule(
            ruleId, ecosystemId, relayId,
            nameResult.Value, @operator, action, isActive,
            createdAt: DateTime.UtcNow);

        return Result<AutomationRule>.Success(rule);
    }

    public Result Update(string rawName, Guid relayId, Operator @operator, RuleAction action)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        Name = nameResult.Value;
        RelayId = relayId;
        Operator = @operator;
        Action = action;

        IncrementVersion();

        return Result.Success();
    }

    public void SetAction(RuleAction action)
    {
        if (Action == action)
        {
            return;
        }

        Action = action;

        IncrementVersion();
    }

    public void SetIsActive(bool isActive)
    {
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;

        IncrementVersion();
    }

    public Result AddCondition(RuleCondition condition)
    {
        if (_ruleConditions.Exists(c => c.SensorId == condition.SensorId))
        {
            return Result.Failure(Error.Conflict<RuleCondition>(
                string.Format(ControlValidationMessages.ConditionForSensorAlreadyExistsFormat, condition.SensorId)));
        }

        _ruleConditions.Add(condition);

        IncrementVersion();

        return Result.Success();
    }

    public Result RemoveCondition(RuleCondition condition)
    {
        if (!_ruleConditions.Exists(x => x.Id == condition.Id))
        {
            return Result.Failure(Error.NotFound<RuleCondition>(
                ControlValidationMessages.ConditionNotFound));
        }

        _ruleConditions.Remove(condition);

        IncrementVersion();

        return Result.Success();
    }

    public bool? EvaluateTargetState(IReadOnlyDictionary<Guid, double> currentSensorData)
    {
        if (!IsActive || _ruleConditions.Count == 0)
        {
            return null;
        }

        var results = new List<bool?>();
        foreach (RuleCondition condition in _ruleConditions)
        {
            if (currentSensorData.TryGetValue(condition.SensorId, out double sensorValue))
            {
                results.Add(condition.Evaluate(sensorValue));
            }
        }

        if (results.Count == 0)
        {
            return null;
        }

        bool? isRuleTriggered = RuleOperatorFactory.CalculateTargetState(results, Operator, Action);
        if (isRuleTriggered == null)
        {
            return null;
        }

        return isRuleTriggered.Value
            ? Action == RuleAction.SwitchOn
            : Action == RuleAction.SwitchOff;
    }
}
