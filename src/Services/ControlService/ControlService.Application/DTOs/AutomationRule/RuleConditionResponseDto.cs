using Contracts.Enums;

namespace Control.Application.DTOs.AutomationRule;

public record RuleConditionResponseDto
{
    public Guid Id { get; init; }
    public Guid SensorId { get; init; }
    public RuleConditionEnum Condition { get; init; }
    public double Threshold { get; init; }
    public double Hysteresis { get; init; }
}
