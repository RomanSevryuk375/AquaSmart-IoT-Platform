using Contracts.Enums;
using Control.Application.DTOs.AutomationRule;

namespace Control.Application.CQRS.AutomationRule.Queries;

public sealed record AutomationRuleDto
{
    public Guid Id { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Guid RelayId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public OperatorEnum Operator { get; private set; }
    public RuleActionEnum Action { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<RuleConditionResponseDto> Conditions { get; private set; } = [];
}
