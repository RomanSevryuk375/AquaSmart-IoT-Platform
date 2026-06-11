using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.CQRS.RuleCondition.Command.UpdateCondition;

public sealed record UpdateConditionCommand 
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid ConditionId { get; init; }
    public Guid SensorId { get; init; }
    public RuleConditionEnum Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}
    