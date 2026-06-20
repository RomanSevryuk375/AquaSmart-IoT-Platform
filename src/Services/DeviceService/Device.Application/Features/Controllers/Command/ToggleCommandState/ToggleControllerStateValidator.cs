// Ignore Spelling: Validator

namespace Device.Application.Features.Controllers.Command.ToggleCommandState;

internal sealed class ToggleControllerStateValidator
    : AbstractValidator<ToggleControllerStateCommand>
{
    public ToggleControllerStateValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
