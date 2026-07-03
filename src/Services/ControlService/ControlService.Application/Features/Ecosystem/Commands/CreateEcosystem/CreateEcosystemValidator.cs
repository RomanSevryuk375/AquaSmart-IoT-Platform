using Contracts.Constants;
using FluentValidation;

namespace Control.Application.Features.Ecosystem.Commands.CreateEcosystem;

public sealed class CreateEcosystemValidator
    : AbstractValidator<CreateEcosystemCommand>
{
    public CreateEcosystemValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(EcosystemConstants.NameLength);

        RuleFor(x => x.Volume)
            .NotNull()
            .GreaterThan(0.0);

        RuleFor(x => x.ControllerId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
