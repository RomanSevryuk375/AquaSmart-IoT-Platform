using Contracts.Constants;
using FluentValidation;

namespace Control.Application.Features.AutomationRules.Commands.CreateRule;

public sealed class CreateRuleValidator
    : AbstractValidator<CreateRuleCommand>
{
    public CreateRuleValidator()
    {
        RuleFor(x => x.EcosystemId)
            .NotEmpty();

        RuleFor(x => x.RelayId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(RuleConstants.NameLength);

        RuleFor(x => x.Operator)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.Action)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.IsActive)
            .NotEmpty();
    }
}
