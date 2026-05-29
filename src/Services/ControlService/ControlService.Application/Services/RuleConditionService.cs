using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using FluentValidation;

namespace Control.Application.Services;

public sealed class RuleConditionService(
    IAutomationRuleRepository ruleRepository,
    IRuleConditionRepository ruleConditionRepository,
    ISensorRepository sensorRepository,
    ISecureService secureService,
    IValidator<RuleConditionRequestDto> validator,
    IUnitOfWork unitOfWork) : IRuleConditionService
{
    public async Task<Result<Guid>> AddConditionAsync(
        Guid ruleId,
        RuleConditionRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Ecosystem.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var ruleResult = await GetValidRuleAsync(ruleId, cancellationToken);
        if (ruleResult.IsFailure)
        {
            return Result<Guid>.Failure(ruleResult.Error);
        }

        var rule = ruleResult.Value;
        var sensorOwrenship = await EnsureConditionOwnsSensorAsync(
            rule, request.SensorId, cancellationToken);
        if (sensorOwrenship.IsFailure)
        {
            return Result<Guid>
                .Failure(sensorOwrenship.Error);
        }

        var result = RuleConditionEntity.Create(
            rule.Id,
            request.SensorId,
            request.Condition,
            request.Threshold,
            request.Hysteresis);

        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error);
        }

        await ruleConditionRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result.Value.Id);
    }

    public async Task<Result> UpdateConditionAsync(
        Guid ruleId,
        Guid conditionId,
        RuleConditionRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(Error.Validation(
                    "Ecosystem.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var ruleResult = await GetValidRuleAsync(ruleId, cancellationToken);
        if (ruleResult.IsFailure)
        {
            return Result.Failure(ruleResult.Error);
        }

        var rule = ruleResult.Value;
        var conditionOwnership = await EnsureRuleOwnsConditionAsync(
            rule, conditionId, cancellationToken);
        if (conditionOwnership.IsFailure)
        {
            return Result.Failure(conditionOwnership.Error);
        }

        var updateResult = conditionOwnership.Value.Update(
            request.Condition,
            request.Threshold,
            request.Hysteresis);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        await ruleConditionRepository.UpdateAsync(conditionOwnership.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteConditionAsync(
        Guid ruleId,
        Guid conditionId,
        CancellationToken cancellationToken)
    {
        var ruleResult = await GetValidRuleAsync(ruleId, cancellationToken);
        if (ruleResult.IsFailure)
        {
            return Result.Failure(ruleResult.Error);
        }

        var rule = ruleResult.Value;
        var conditionOwnership = await EnsureRuleOwnsConditionAsync(
            rule, conditionId, cancellationToken);
        if (conditionOwnership.IsFailure)
        {
            return Result.Failure(conditionOwnership.Error);
        }

        await ruleConditionRepository.DeleteAsync(conditionId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<RuleConditionEntity>> EnsureRuleOwnsConditionAsync(
        AutomationRuleEntity rule,
        Guid conditionId, 
        CancellationToken cancellationToken)
    {
        var condition = await ruleConditionRepository.GetByIdAsync(
            conditionId, cancellationToken);
        if (condition is null)
        {
            return Result<RuleConditionEntity>
                .Failure(Error.NotFound(
                    "Condition.NotFound",
                    "Condition not found"));
        }

        if (condition.AutomationRuleId != rule.Id)
        {
            return Result<RuleConditionEntity>
                .Failure(Error.Conflict(
                    "Condition.Forbidden",
                    "Rule do not contains this condition"));
        }

        return Result<RuleConditionEntity>.Success(condition);
    }

    private async Task<Result> EnsureConditionOwnsSensorAsync(
        AutomationRuleEntity rule,
        Guid sensorId, 
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(sensorId, cancellationToken);
        if (sensor is null)
        {
            return Result.Failure(Error.NotFound(
                    "Sensor.NotFound",
                    "Sensor not found"));
        }

        if (sensor.EcosystemId != rule.EcosystemId)
        {
            return Result.Failure(Error.Validation(
                    "Condition.InvalidSensor",
                    "Sensor must belong to the same ecosystem as the rule"));
        }

        return Result.Success();
    }

    private async Task<Result<AutomationRuleEntity>> GetValidRuleAsync(
        Guid ruleId, 
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdWithConditionsAsync(
            ruleId, cancellationToken);
        if (rule is null)
        {
            return Result<AutomationRuleEntity>
                .Failure(Error.NotFound(
                    "Rule.NotFound",
                    $"Rule {ruleId} not found"));
        }

        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            rule.EcosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<AutomationRuleEntity>.Failure(ownership.Error);
        }

        return Result<AutomationRuleEntity>.Success(rule);
    }
}
