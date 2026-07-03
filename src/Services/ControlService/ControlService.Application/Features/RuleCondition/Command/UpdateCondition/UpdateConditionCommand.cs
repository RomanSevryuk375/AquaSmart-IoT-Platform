using Contracts.Abstractions;

namespace Control.Application.Features.RuleCondition.Command.UpdateCondition;

public sealed record UpdateConditionCommand
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid ConditionId { get; init; }
    public Guid SensorId { get; init; }
    public Contracts.Enums.RuleCondition Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}
