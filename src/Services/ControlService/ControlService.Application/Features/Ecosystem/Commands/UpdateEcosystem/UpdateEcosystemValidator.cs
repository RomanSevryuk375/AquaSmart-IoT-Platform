using Contracts.Constants;
using FluentValidation;

namespace Control.Application.Features.Ecosystem.Commands.UpdateEcosystem;

public sealed class UpdateEcosystemValidator
    : AbstractValidator<UpdateEcosystemCommand>
{
    public UpdateEcosystemValidator()
    {
        RuleFor(x => x.EcosystemId)
            .NotEmpty();

        RuleFor(x => x.Volume)
            .GreaterThan(0.0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(EcosystemConstants.NameLength);
    }
}
