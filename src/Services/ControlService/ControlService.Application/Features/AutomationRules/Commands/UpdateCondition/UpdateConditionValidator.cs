// Ignore Spelling: Validator

using FluentValidation;

namespace Control.Application.Features.AutomationRules.Commands.UpdateCondition;

public sealed class UpdateConditionValidator
    : AbstractValidator<UpdateConditionCommand>
{
    public UpdateConditionValidator()
    {
        RuleFor(x => x.RuleId)
            .NotEmpty();

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
