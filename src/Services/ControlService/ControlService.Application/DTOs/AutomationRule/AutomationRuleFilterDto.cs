using Contracts.Enums;

namespace Control.Application.DTOs.AutomationRule;

public record AutomationRuleFilterDto
{
    public Guid? EcosystemId { get; init; }
    public Guid? RelayId { get; init; }
    public RuleActionEnum? Action { get; init; }
    public OperatorEnum? Operator { get; init; }
}
