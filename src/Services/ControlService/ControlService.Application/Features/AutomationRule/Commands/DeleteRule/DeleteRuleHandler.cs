using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.AutomationRule.Commands.DeleteRule;

public sealed class DeleteRuleHandler(
    IAutomationRuleRepository ruleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteRuleCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRuleCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.AutomationRule? rule = await ruleRepository.GetByIdAsync(request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result<Domain.Entities.AutomationRule>.Failure(Error.NotFound(
                "Rule.NotFound", $"Rule {request.RuleId} not found"));
        }

        await ruleRepository.DeleteAsync(rule.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
