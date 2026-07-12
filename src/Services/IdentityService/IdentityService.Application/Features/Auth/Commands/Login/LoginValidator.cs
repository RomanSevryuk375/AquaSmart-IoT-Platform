// Ignore Spelling: Validator

using Contracts.Constants;
using FluentValidation;

namespace IdentityService.Application.Features.Auth.Commands.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(IdentityConstants.EmailLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(IdentityConstants.PasswordLength)
            .WithMessage(ValidationMessages.PasswordMinLength);
    }
}
