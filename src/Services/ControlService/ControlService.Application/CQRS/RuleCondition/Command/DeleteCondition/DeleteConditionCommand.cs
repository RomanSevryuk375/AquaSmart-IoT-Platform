using Contracts.Abstractions;
using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.RuleCondition.Command.DeleteCondition;

public sealed record DeleteConditionCommand 
    : IRequest<Result>, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid ConditionId { get; init; }
}
