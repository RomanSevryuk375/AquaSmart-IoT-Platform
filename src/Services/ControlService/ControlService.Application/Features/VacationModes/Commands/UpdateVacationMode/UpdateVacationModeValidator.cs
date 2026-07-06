// Ignore Spelling: Validator

using FluentValidation;

namespace Control.Application.Features.VacationModes.Commands.UpdateVacationMode;

public sealed class UpdateVacationModeValidator : AbstractValidator<UpdateVacationModeCommand>
{
    public UpdateVacationModeValidator()
    {
        RuleFor(x => x.VacationModeId)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate);

        RuleFor(x => x.CalculatedFeed)
            .GreaterThanOrEqualTo(0);
    }
}
