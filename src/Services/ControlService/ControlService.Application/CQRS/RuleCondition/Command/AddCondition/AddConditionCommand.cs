using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.CQRS.RuleCondition.Command.AddCondition;

public sealed record AddConditionCommand 
    : ICommand<Guid>, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid SensorId { get; init; }
    public RuleConditionEnum Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}
