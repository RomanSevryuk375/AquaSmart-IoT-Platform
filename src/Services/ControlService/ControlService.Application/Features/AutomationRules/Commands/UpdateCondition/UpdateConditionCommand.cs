using Contracts.Abstractions;
using Contracts.Enums;
using Control.Application.Interfaces;

namespace Control.Application.Features.AutomationRules.Commands.UpdateCondition;

public sealed record UpdateConditionCommand
    : ICommand, IRuleBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid ConditionId { get; init; }
    public Guid SensorId { get; init; }
    public Condition Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}
