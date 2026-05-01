using Contracts.Enums;

namespace Control.Application.DTOs.AutomationRule;

public record RuleConditionRequestDto
{
    public Guid SensorId { get; init; }
    public RuleConditionEnum Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}