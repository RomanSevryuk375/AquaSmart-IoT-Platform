using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using FluentValidation;

namespace Control.Application.Services;

public class RuleConditionService(
    IAutomationRuleRepository ruleRepository,
    IRuleConditionRepository ruleConditionRepository,
    IEcosystemRepository ecosystemRepository,
    ISensorRepository sensorRepository,
    IUserContext userContext,
    IValidator<RuleConditionRequestDto> validator,
    IUnitOfWork unitOfWork) : IRuleConditionService
{
    public async Task<Result<Guid>> AddConditionAsync(
        Guid ruleId,
        RuleConditionRequestDto request,
        CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);

        var rule = await ruleRepository
            .GetByIdWithConditionsAsync(ruleId, cancellationToken);

        if (rule is null)
        {
            return Result<Guid>.Failure(
                Error.NotFound(
                    "Rule.NotFound", 
                    $"Rule {ruleId} not found"));
        }

        var ecosystem = await ecosystemRepository
            .GetByIdAsync(rule.EcosystemId, cancellationToken);
        
        if (ecosystem is null || 
            ecosystem.UserId != userContext.UserId)
        {
            return Result<Guid>.Failure(
                Error.Conflict(
                    "Access.Denied", 
                    "You do not own this ecosystem"));
        }

        var sensor = await sensorRepository
            .GetByIdAsync(request.SensorId, cancellationToken);

        if (sensor is null)
        {
            return Result<Guid>.Failure(
                Error.NotFound(
                    "Sensor.NotFound", 
                    "Sensor not found"));
        }

        if (sensor.EcosystemId != rule.EcosystemId)
        {
            return Result<Guid>.Failure(
                Error.Validation(
                    "Condition.InvalidSensor", 
                    "Sensor must belong to the same ecosystem as the rule"));
        }

        var (condition, errors) = RuleConditionEntity.Create(
            rule.Id,
            request.SensorId,
            request.Condition,
            request.Threshold,
            request.Hysteresis);

        if (condition is null)
        {
            return Result<Guid>.Failure(
                Error.Validation(
                    "Condition.Invalid", 
                    string.Join(", ", errors!)));
        }

        await ruleConditionRepository.AddAsync(condition, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(condition.Id);
    }

    public async Task<Result> UpdateConditionAsync(
        Guid ruleId,
        Guid conditionId,
        RuleConditionRequestDto request,
        CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);

        var rule = await ruleRepository
            .GetByIdWithConditionsAsync(ruleId, cancellationToken);

        if (rule is null)
        {
            return Result
                .Failure(Error.NotFound(
                    "Rule.NotFound",
                    "Rule not found"));
        }

        var condition = await ruleConditionRepository
            .GetByIdAsync(conditionId, cancellationToken);

        if (condition is null)
        {
            return Result
                .Failure(Error.NotFound(
                    "Condition.NotFound",
                    "Condition not found"));
        }

        if (rule.Conditions.Any(x => x.Id == condition.AutomationRuleId))
        {
            return Result
                .Failure(Error.Conflict(
                    "Condition.Forbidden",
                    "Rule do not contains this condition"));
        }

        var ecosystem = await ecosystemRepository
            .GetByIdAsync(rule.EcosystemId, cancellationToken);

        if (ecosystem is null ||
            ecosystem.UserId != userContext.UserId)
        {
            return Result
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "Access denied"));
        }

        var updateErrors = condition.Update(
            request.Condition,
            request.Threshold,
            request.Hysteresis);

        if (updateErrors is not null)
        {
            return Result
                .Failure(Error.Validation(
                    "Condition.UpdateFailed",
                    string.Join(", ", updateErrors)));
        }

        await ruleConditionRepository.UpdateAsync(condition, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteConditionAsync(
        Guid ruleId,
        Guid conditionId,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository
            .GetByIdWithConditionsAsync(ruleId, cancellationToken);

        if (rule is null)
        {
            return Result
                .Failure(Error.NotFound(
                    "Rule.NotFound",
                    "Rule not found"));
        }

        var condition = await ruleConditionRepository
            .GetByIdAsync(conditionId, cancellationToken);

        if (condition is null)
        {
            return Result
                .Failure(Error.NotFound(
                    "Condition.NotFound",
                    "Condition not found"));
        }

        if (rule.Conditions.Any(x => x.Id == condition.AutomationRuleId))
        {
            return Result
                .Failure(Error.Conflict(
                    "Condition.Forbidden",
                    "Rule do not contains this condition"));
        }

        var ecosystem = await ecosystemRepository
            .GetByIdAsync(rule.EcosystemId, cancellationToken);

        if (ecosystem is null ||
            ecosystem.UserId != userContext.UserId)
        {
            return Result
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "Access denied"));
        }

        await ruleConditionRepository.DeleteAsync(ruleId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
