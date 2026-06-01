using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.AutomationRule.Commands.UpdateRule;

public sealed record UpdateRuleCommand 
    : IRequest<Result>, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid RelayId { get; init; }
    public OperatorEnum Operator { get; init; }
    public RuleActionEnum Action { get; init; }
}
