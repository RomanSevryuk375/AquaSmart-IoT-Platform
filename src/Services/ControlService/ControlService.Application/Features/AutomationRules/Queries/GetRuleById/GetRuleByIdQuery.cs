using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Interfaces;

namespace Control.Application.Features.AutomationRules.Queries.GetRuleById;

public sealed record GetRuleByIdQuery
    : IQuery<Result<AutomationRuleDto>>, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
}
