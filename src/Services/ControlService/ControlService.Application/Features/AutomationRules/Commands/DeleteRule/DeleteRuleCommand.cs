using Contracts.Abstractions;

namespace Control.Application.Features.AutomationRules.Commands.DeleteRule;

public sealed record DeleteRuleCommand
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
}
