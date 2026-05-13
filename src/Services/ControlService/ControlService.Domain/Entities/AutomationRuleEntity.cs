using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;

namespace Control.Domain.Entities;

public class AutomationRuleEntity : IEntity
{
    private AutomationRuleEntity(
        Guid id,
        Guid ecosystemId,
        Guid relayId,
        string name,
        OperatorEnum @operator,
        RuleActionEnum action, 
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

    public Guid Id { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Guid RelayId { get; private set; }
    public string Name { get; private set; }
    public OperatorEnum Operator { get; private set; }
    public RuleActionEnum Action { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual ICollection<RuleConditionEntity> Conditions { get; private set; } = [];

    public static Result<AutomationRuleEntity> Create(
        Guid ecosystemId,
        string name,
        Guid relayId,
        OperatorEnum @operator,
        RuleActionEnum action,
        bool isActive)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name must not be empty.");
        }

        if (ecosystemId == Guid.Empty)
        {
            errors.Add("ecosystemId must not be empty.");
        }

        if (relayId == Guid.Empty)
        {
            errors.Add("relayId must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result<AutomationRuleEntity>.Failure(
                Error.Validation(
                    "Rule.Invalid",
                    string.Join("; ", errors)));
        }

        var rule = new AutomationRuleEntity(
            Guid.NewGuid(),
            ecosystemId,
            relayId,
            name,
            @operator,
            action,
            isActive,
            DateTime.UtcNow);

        return Result<AutomationRuleEntity>.Success(rule);
    }

    public Result Update (
        string name,
        Guid relayId,
        OperatorEnum @operator,
        RuleActionEnum action)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name must not be empty.");
        }

        if (relayId == Guid.Empty)
        {
            errors.Add("relayId must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result.Failure(Error.Validation(
                     "Rule.Invalid",
                     string.Join("; ", errors)));
        }

        Name = name;
        RelayId = relayId;
        Operator = @operator;
        Action = action;

        return Result.Success();
    }

    public void SetAction(RuleActionEnum action)
    {
        if (Action == action)
        {
            return;
        }

        Action = action;
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void AddCondition(RuleConditionEntity condition)
    {
        if (condition is null)
        {
            return;
        }

        Conditions.Add(condition);
    }
}
