// Ignore Spelling: Validator

using FluentValidation;

namespace IdentityService.Application.Features.Auth.Commands.TelegramLogin;

public sealed class TelegramLoginValidator : AbstractValidator<TelegramLoginCommand>
{
    public TelegramLoginValidator()
    {
        RuleFor(x => x.ChatId)
            .GreaterThan(0)
            .NotEmpty();
    }
}
