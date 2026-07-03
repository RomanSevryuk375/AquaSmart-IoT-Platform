using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.Features.AutomationRules.Commands.UpdateRule;

public sealed record UpdateRuleCommand
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid RelayId { get; init; }
    public Operator Operator { get; init; }
    public RuleAction Action { get; init; }
}
