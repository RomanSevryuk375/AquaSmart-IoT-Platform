using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace Control.Application.Features.AutomationRules.Commands.AddCondition;

public sealed class AddConditionHandler(IAutomationRuleRepository ruleRepository)
    : IRequestHandler<AddConditionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddConditionCommand request,
        CancellationToken cancellationToken)
    {
        AutomationRule? rule = await ruleRepository.GetByIdWithConditionsAsync(
            request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result<Guid>.Failure(Error.NotFound<AutomationRule>(
                $"Rule {request.RuleId} not found"));
        }

        Result<RuleCondition> result = RuleCondition.Create(
            ruleConditionId: NewId.NextGuid(), rule.Id, request.SensorId,
            request.Condition, request.Threshold, request.Hysteresis);
        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error);
        }

        Result addConditionResult = rule.AddCondition(result.Value);
        if (addConditionResult.IsFailure)
        {
            return Result<Guid>.Failure(addConditionResult.Error);
        }

        return Result<Guid>.Success(result.Value.Id);
    }
}
