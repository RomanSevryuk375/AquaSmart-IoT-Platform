// Ignore Spelling: Validator

using Contracts.Constants;
using FluentValidation;

namespace IdentityService.Application.Features.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(CommonConstants.NameLength);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(IdentityConstants.PhoneNumberRegex)
            .WithMessage(ValidationMessages.PhoneFormatInvalid);
    }
}
