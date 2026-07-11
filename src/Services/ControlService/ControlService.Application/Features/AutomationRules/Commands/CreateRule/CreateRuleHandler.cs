using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace Control.Application.Features.AutomationRules.Commands.CreateRule;

public sealed class CreateRuleHandler(
    IAutomationRuleRepository ruleRepository,
    IHardwareValidator hardwareValidator)
    : IRequestHandler<CreateRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateRuleCommand request,
        CancellationToken cancellationToken)
    {
        Result<AutomationRule> createResult = AutomationRule.Create(
            ruleId: NewId.NextGuid(), request.EcosystemId, request.Name, request.RelayId,
            request.Operator, request.Action, request.IsActive);
        if (createResult.IsFailure)
        {
            return Result<Guid>.Failure(createResult.Error);
        }

        AutomationRule rule = createResult.Value;

        foreach (RuleConditionRequestDto conditionDto in request.Conditions)
        {
            Result validationResult = await hardwareValidator.ValidateAssignmentAsync(
                conditionDto.SensorId, request.RelayId, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result<Guid>.Failure(validationResult.Error);
            }

            Result<RuleCondition> conditionResult = RuleCondition.Create(
                ruleConditionId: NewId.NextGuid(), rule.Id, conditionDto.SensorId,
                conditionDto.Condition, conditionDto.Threshold, conditionDto.Hysteresis);
            if (conditionResult.IsFailure)
            {
                return Result<Guid>.Failure(conditionResult.Error);
            }

            Result addConditionResult = rule.AddCondition(conditionResult.Value);
            if (addConditionResult.IsFailure)
            {
                return Result<Guid>.Failure(addConditionResult.Error);
            }
        }

        Guid resultId = await ruleRepository.AddAsync(rule, cancellationToken);

        return Result<Guid>.Success(resultId);
    }
}
