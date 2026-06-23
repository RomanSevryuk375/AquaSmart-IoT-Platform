using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;

namespace Control.Application.CQRS.AutomationRule.Queries.GetAllRules;

public sealed record GetAllRulesQuery 
    : IQuery<Result<IReadOnlyList<AutomationRuleDto>>>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public Guid? RelayId { get; init; }
    public RuleAction? Action { get; init; }
    public Operator? Operator { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
