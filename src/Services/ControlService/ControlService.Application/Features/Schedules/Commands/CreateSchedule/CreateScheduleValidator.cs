// Ignore Spelling: Validator

using FluentValidation;

namespace Control.Application.Features.Schedules.Commands.CreateSchedule;

public sealed class CreateScheduleValidator
    : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleValidator()
    {
        RuleFor(x => x.EcosystemId)
            .NotEmpty();

        RuleFor(x => x.RelayId)
            .NotEmpty();

        RuleFor(x => x.CronExpression)
            .NotEmpty();

        RuleFor(x => x.DurationMin)
            .GreaterThan(0);
    }
}
