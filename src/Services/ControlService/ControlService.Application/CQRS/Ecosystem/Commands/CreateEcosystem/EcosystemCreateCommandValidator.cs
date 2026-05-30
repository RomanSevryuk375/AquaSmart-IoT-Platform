using Contracts.Constants;
using FluentValidation;

namespace Control.Application.CQRS.Ecosystem.Commands.CreateEcosystem;

public sealed class EcosystemCreateCommandValidator 
    : AbstractValidator<CreateEcosystemCommand>
{
    public EcosystemCreateCommandValidator() 
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
