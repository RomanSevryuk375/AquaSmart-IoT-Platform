using Contracts.Abstractions;
using Contracts.Enums;
using Control.Application.Interfaces;

namespace Control.Application.Features.AutomationRules.Commands.AddCondition;

public sealed record AddConditionCommand
    : ICommand<Guid>, IRuleSensorBoundRequest
{
    public Guid RuleId { get; init; }
    public Guid SensorId { get; init; }
    public Condition Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}
