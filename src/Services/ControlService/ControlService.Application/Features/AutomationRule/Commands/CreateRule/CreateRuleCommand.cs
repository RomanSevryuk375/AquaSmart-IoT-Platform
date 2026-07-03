using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.Features.AutomationRule.Commands.CreateRule;

public sealed record CreateRuleCommand
    : ICommand<Guid>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public Guid RelayId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Operator Operator { get; init; }
    public RuleAction Action { get; init; }
    public bool IsActive { get; init; }
}
