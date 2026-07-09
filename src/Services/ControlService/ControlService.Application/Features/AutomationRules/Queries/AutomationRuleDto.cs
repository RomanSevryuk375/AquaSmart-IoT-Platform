// Ignore Spelling: Dto

using Contracts.Enums;
using Control.Application.DTOs.AutomationRule;

namespace Control.Application.Features.AutomationRules.Queries;

public sealed record AutomationRuleDto
{
    public Guid Id { get; init; }
    public Guid EcosystemId { get; init; }
    public Guid RelayId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Operator Operator { get; init; }
    public RuleAction Action { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }

    public IReadOnlyList<RuleConditionResponseDto> Conditions { get; init; } = [];
}
