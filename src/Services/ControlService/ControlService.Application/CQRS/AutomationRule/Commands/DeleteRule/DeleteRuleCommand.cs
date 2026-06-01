using Contracts.Abstractions;
using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.AutomationRule.Commands.DeleteRule;

public sealed record DeleteRuleCommand 
    : IRequest<Result>, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
}
