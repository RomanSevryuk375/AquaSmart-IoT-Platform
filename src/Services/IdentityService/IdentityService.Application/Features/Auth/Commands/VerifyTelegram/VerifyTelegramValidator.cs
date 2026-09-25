// Ignore Spelling: Validator

using FluentValidation;

namespace IdentityService.Application.Features.Auth.Commands.VerifyTelegram;

public sealed class VerifyTelegramValidator : AbstractValidator<VerifyTelegramCommand>
{
    public VerifyTelegramValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.ChatId)
            .GreaterThan(0)
            .NotEmpty();
    }
}
