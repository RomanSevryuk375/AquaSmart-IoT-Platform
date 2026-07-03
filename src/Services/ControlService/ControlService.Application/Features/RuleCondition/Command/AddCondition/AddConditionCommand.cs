using Contracts.Abstractions;

namespace Control.Application.Features.RuleCondition.Command.AddCondition;

public sealed record AddConditionCommand
    : ICommand<Guid>, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid SensorId { get; init; }
    public Contracts.Enums.RuleCondition Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}
