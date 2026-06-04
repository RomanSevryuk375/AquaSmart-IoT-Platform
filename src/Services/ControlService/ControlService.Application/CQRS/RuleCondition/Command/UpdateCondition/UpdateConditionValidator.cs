using FluentValidation;

namespace Control.Application.CQRS.RuleCondition.Command.UpdateCondition;

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
