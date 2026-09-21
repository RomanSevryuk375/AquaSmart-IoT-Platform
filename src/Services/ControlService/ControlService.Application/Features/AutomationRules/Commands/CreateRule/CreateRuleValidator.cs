// Ignore Spelling: Validator

using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
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

        RuleFor(x => x.Conditions)
            .Must(c => c.Count == 1)
            .When(x => x.Operator == Operator.NOT)
            .WithMessage("Operator NOT can only have exactly one condition.");
    }
}
