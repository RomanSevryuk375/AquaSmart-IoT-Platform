using Contracts.Abstractions;
using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.AutomationRule.Queries.GetRuleById;

public sealed record GetRuleByIdQuery 
    : IRequest<Result<AutomationRuleDto>>, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
}
