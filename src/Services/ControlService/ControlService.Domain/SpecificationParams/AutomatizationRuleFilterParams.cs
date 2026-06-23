using Contracts.Enums;

namespace Control.Domain.SpecificationParams;

public sealed record AutomationRuleFilterParams
{
    public Guid? EcosystemId { get; init; }
    public Guid? RelayId { get; init; }
    public RuleAction? Action { get; init; }
    public Operator? Operator { get; init; }
}
