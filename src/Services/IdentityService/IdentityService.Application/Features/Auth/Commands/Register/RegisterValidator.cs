// Ignore Spelling: Validator

using Contracts.Constants;
using FluentValidation;

namespace IdentityService.Application.Features.Auth.Commands.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(IdentityConstants.EmailLength);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(CommonConstants.NameLength);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(IdentityConstants.PhoneNumberRegex)
            .WithMessage("Phone number should be in format +375XXXXXXXXX or 80XXXXXXXXX");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(IdentityConstants.PasswordLength)
            .WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.TimeZone)
            .NotEmpty();
    }
}
