using Contracts.Constants;
using Control.Application.DTOs.AutomationRule;
using FluentValidation;

namespace Control.Application.DTOs.Validators;

public sealed class AutomationRuleRequestDtoValidator
    : AbstractValidator<AutomationRuleRequestDto>
{
    public AutomationRuleRequestDtoValidator()
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
