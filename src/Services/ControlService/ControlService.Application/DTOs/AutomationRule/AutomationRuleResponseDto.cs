using Contracts.Enums;

namespace Control.Application.DTOs.AutomationRule;

public record AutomationRuleResponseDto
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
