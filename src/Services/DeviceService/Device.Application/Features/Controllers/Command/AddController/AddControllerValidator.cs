// Ignore Spelling: Validator

namespace Device.Application.Features.Controllers.Command.AddController;

internal class AddControllerValidator
    : AbstractValidator<AddControllerCommand>
{
    public AddControllerValidator()
    {
        RuleFor(x => x.MacAddress)
            .NotEmpty()
            .Length(ControllerConstants.MacAddressLength)
            .Matches(ControllerConstants.MacAddressRegex);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(CommonConstants.NameLength);

        RuleFor(x => x.IsOnline)
            .NotEmpty();
    }
}
