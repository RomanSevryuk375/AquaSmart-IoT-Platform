using Contracts.Results;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.AutomationRules.Commands.DeleteRule;

public sealed class DeleteRuleHandler(IAutomationRuleRepository ruleRepository)
    : IRequestHandler<DeleteRuleCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRuleCommand request,
        CancellationToken cancellationToken)
    {
        await ruleRepository.DeleteAsync(request.RuleId, cancellationToken);

        return Result.Success();
    }
}
