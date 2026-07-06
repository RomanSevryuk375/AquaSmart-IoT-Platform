using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.AutomationRules.Commands.DeleteCondition;

public sealed record DeleteConditionCommand
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid ConditionId { get; init; }
}
