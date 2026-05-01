using Control.Application.DTOs.AutomationRule;
using FluentValidation;

namespace Control.Application.DTOs.Validators;

public class RuleConditionRequestDtoValidator 
    : AbstractValidator<RuleConditionRequestDto>
{
    public RuleConditionRequestDtoValidator()
    {
        RuleFor(x => x.SensorId)
            .NotEmpty();

        RuleFor(x => x.Condition)
            .IsInEnum()
            .NotEmpty();

        RuleFor(x => x.Threshold)
            .NotEmpty();

        RuleFor(x => x.Hysteresis)
            .NotEmpty();
    }
}
