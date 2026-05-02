using Contracts.Constants;
using Control.Application.DTOs.Ecosystem;
using FluentValidation;

namespace Control.Application.DTOs.Validators;

public sealed class EcosystemRequestDtoValidator 
    : AbstractValidator<EcosystemRequestDto>
{
    public EcosystemRequestDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(EcosystemConstants.NameLength);

        RuleFor(x => x.Volume)
            .NotEmpty()
            .GreaterThan(0.0);

        RuleFor(x => x.ControllerId)
            .NotEmpty();
    }
}
