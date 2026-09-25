// Ignore Spelling: Validator

using FluentValidation;

namespace IdentityService.Application.Features.Auth.Commands.GenerateTelegramLink;

public sealed class GenerateTelegramLinkValidator : AbstractValidator<GenerateTelegramLinkCommand>
{
    public GenerateTelegramLinkValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
