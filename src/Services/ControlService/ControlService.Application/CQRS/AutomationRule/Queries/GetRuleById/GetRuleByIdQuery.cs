using Contracts.Abstractions;
using Contracts.Results;

namespace Control.Application.CQRS.AutomationRule.Queries.GetRuleById;

public sealed record GetRuleByIdQuery 
    : IQuery<Result<AutomationRuleDto>>, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
}
