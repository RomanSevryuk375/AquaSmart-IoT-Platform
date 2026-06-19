using Contracts.Constants;

namespace Device.Application.DTOs.Validators.ControllerValidators;

public sealed class ControllerUpdateRequestDtoValidator
    : AbstractValidator<ControllerUpdateRequestDto>
{
    public ControllerUpdateRequestDtoValidator()
    {
        RuleFor(x => x.MacAddress)
           .NotEmpty()
           .Matches(ControllerConstants.MacAddressRegex)
           .WithMessage("Invalid MacAddress format.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ControllerConstants.NameLength);
    }
}
