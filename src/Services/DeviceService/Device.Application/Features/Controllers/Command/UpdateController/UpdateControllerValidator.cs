// Ignore Spelling: Validator

namespace Device.Application.Features.Controllers.Command.UpdateController;

internal sealed class UpdateControllerValidator
    : AbstractValidator<UpdateControllerCommand>
{
    public UpdateControllerValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.MacAddress)
           .NotEmpty()
           .Matches(ControllerConstants.MacAddressRegex)
           .WithMessage("Invalid MacAddress format.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(CommonConstants.NameLength);
    }
}
