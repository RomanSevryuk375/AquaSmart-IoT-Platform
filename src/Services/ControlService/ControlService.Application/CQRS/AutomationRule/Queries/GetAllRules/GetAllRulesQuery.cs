using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.AutomationRule.Queries.GetAllRules;

public sealed record GetAllRulesQuery 
    : IRequest<Result<IReadOnlyList<AutomationRuleDto>>>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public Guid? RelayId { get; init; }
    public RuleActionEnum? Action { get; init; }
    public OperatorEnum? Operator { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
