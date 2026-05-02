using Contracts.Constants;
using Device.Application.DTOs.Controller;
using FluentValidation;

namespace Device.Application.DTOs.Validators.ControllerValidators;

public sealed class ControllerRequestDtoValidator
    : AbstractValidator<ControllerRequestDto>
{
    public ControllerRequestDtoValidator()
    {
        RuleFor(x => x.MacAddress)
            .NotEmpty()
            .Matches(ControllerConstants.MacAddressRegex)
            .WithMessage("Invalid MacAddress format.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ControllerConstants.NameLength);

        RuleFor(x => x.IsOnline)
            .NotEmpty();
    }
}
