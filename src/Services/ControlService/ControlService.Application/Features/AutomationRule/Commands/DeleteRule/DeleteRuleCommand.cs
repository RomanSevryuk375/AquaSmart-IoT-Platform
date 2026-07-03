using Contracts.Abstractions;

namespace Control.Application.Features.AutomationRule.Commands.DeleteRule;

public sealed record DeleteRuleCommand
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
}
