using Contracts.Abstractions;

namespace Control.Application.CQRS.AutomationRule.Commands.DeleteRule;

public sealed record DeleteRuleCommand 
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
}
