using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.CQRS.AutomationRule.Commands.UpdateRule;

public sealed record UpdateRuleCommand 
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid RelayId { get; init; }
    public OperatorEnum Operator { get; init; }
    public RuleActionEnum Action { get; init; }
}
