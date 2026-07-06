// Ignore Spelling: Validator

using FluentValidation;

namespace Control.Application.Features.VacationModes.Commands.CreateVacationMode;

public sealed class CreateVacationModeValidator
    : AbstractValidator<CreateVacationModeCommand>
{
    public CreateVacationModeValidator()
    {
        RuleFor(x => x.EcosystemId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate);
        RuleFor(x => x.CalculatedFeed).GreaterThanOrEqualTo(0);
    }
}
