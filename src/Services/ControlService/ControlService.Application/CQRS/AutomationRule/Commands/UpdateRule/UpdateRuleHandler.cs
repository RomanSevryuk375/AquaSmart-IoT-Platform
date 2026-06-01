using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.CQRS.AutomationRule.Commands.UpdateRule;

public sealed class UpdateRuleHandler(
    IAutomationRuleRepository ruleRepository, 
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRuleCommand, Result>
{
    public async Task<Result> Handle(
        UpdateRuleCommand request, 
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdAsync(request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result<AutomationRuleEntity>.Failure(Error.NotFound(
                "Rule.NotFound", $"Rule {request.RuleId} not found"));
        }

        var validationResult = rule.Update(
            request.Name, request.RelayId, request.Operator, request.Action);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        await ruleRepository.UpdateAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
