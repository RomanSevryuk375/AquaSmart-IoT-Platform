using Contracts.Abstractions;

namespace Control.Application.Features.RuleConditions.Command.DeleteCondition;

public sealed record DeleteConditionCommand
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid ConditionId { get; init; }
}
