using FluentValidation;

namespace Control.Application.Features.RuleCondition.Command.AddCondition;

public sealed class AddConditionValidator
    : AbstractValidator<AddConditionCommand>
{
    public AddConditionValidator()
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
