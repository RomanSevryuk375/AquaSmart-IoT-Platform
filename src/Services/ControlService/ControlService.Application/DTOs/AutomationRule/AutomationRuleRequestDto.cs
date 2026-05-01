using Contracts.Enums;

namespace Control.Application.DTOs.AutomationRule;

public record AutomationRuleRequestDto
{
    public Guid EcosystemId { get; init; }
    public Guid RelayId { get; init; }
    public string Name { get; init; } = string.Empty;
    public OperatorEnum Operator { get; init; }
    public RuleActionEnum Action { get; init; }
    public bool IsActive { get; init; }
}
