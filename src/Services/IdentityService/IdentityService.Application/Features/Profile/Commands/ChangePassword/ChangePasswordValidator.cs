// Ignore Spelling: Validator

using FluentValidation;

namespace IdentityService.Application.Features.Profile.Commands.ChangePassword;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");
    }
}
