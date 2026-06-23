using Contracts.Enums;

namespace Control.Application.DTOs.AutomationRule;

public record AutomationRuleUpdateRequestDto
{
    public string Name { get; init; } = string.Empty;
    public Guid RelayId { get; init; }
    public Operator Operator { get; init; }
    public RuleAction Action { get; init; }
}
