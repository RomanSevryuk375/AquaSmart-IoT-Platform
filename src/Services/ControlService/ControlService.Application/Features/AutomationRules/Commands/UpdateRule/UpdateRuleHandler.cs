using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.AutomationRules.Commands.UpdateRule;

public sealed class UpdateRuleHandler(IAutomationRuleRepository ruleRepository)
    : IRequestHandler<UpdateRuleCommand, Result>
{
    public async Task<Result> Handle(
        UpdateRuleCommand request,
        CancellationToken cancellationToken)
    {
        AutomationRule? rule = await ruleRepository.GetByIdAsync(request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result<AutomationRule>.Failure(Error.NotFound<RuleCondition>(
                $"Rule {request.RuleId} not found"));
        }

        Result validationResult = rule.Update(
            request.Name, request.RelayId, request.Operator, request.Action);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        return Result.Success();
    }
}
