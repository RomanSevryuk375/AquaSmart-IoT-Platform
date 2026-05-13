using Contracts.Enums;

namespace Control.Domain.SpecificationParams;

public sealed record AutomationRuleFilterParams
{
    public Guid? EcosystemId { get; init; }
    public Guid? RelayId { get; init; }
    public RuleActionEnum? Action { get; init; }
    public OperatorEnum? Operator { get; init; }
}
