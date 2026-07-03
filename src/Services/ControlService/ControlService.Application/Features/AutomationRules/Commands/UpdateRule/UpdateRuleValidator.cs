using Contracts.Constants;
using FluentValidation;

namespace Control.Application.Features.AutomationRules.Commands.UpdateRule;

public sealed class UpdateRuleValidator
    : AbstractValidator<UpdateRuleCommand>
{
    public UpdateRuleValidator()
    {
        RuleFor(x => x.RuleId)
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
    }
}
