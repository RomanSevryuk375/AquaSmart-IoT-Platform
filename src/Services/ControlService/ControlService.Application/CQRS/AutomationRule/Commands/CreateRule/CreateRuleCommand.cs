using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.AutomationRule.Commands.CreateRule;

public sealed record CreateRuleCommand 
    : IRequest<Result<Guid>>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public Guid RelayId { get; init; }
    public string Name { get; init; } = string.Empty;
    public OperatorEnum Operator { get; init; }
    public RuleActionEnum Action { get; init; }
    public bool IsActive { get; init; }
}
