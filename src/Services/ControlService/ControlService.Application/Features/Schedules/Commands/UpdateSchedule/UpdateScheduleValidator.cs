// Ignore Spelling: Validator

using FluentValidation;

namespace Control.Application.Features.Schedules.Commands.UpdateSchedule;

public sealed class UpdateScheduleValidator
    : AbstractValidator<UpdateScheduleCommand>
{
    public UpdateScheduleValidator()
    {
        RuleFor(x => x.CronExpression)
            .NotEmpty();

        RuleFor(x => x.DurationMin)
            .GreaterThan(0);
    }
}
