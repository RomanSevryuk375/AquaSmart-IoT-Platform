using Contracts.Abstractions;

namespace Control.Application.Features.RuleConditions.Command.UpdateCondition;

public sealed record UpdateConditionCommand
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid ConditionId { get; init; }
    public Guid SensorId { get; init; }
    public Contracts.Enums.Condition Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}
