using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.RuleCondition.Command.AddCondition;

public sealed class AddConditionHandler(
    ISensorRepository sensorRepository,
    IRuleConditionRepository ruleConditionRepository,
    IAutomationRuleRepository ruleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddConditionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddConditionCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.AutomationRule? rule = await ruleRepository.GetByIdWithConditionsAsync(
            request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result<Guid>.Failure(Error.NotFound(
                    "Rule.NotFound", $"Rule {request.RuleId} not found"));
        }

        Result sensorOwrenship = await EnsureConditionOwnsSensorAsync(
            rule, request.SensorId, cancellationToken);
        if (sensorOwrenship.IsFailure)
        {
            return Result<Guid>
                .Failure(sensorOwrenship.Error);
        }

        Result<RuleConditionEntity> result = RuleConditionEntity.Create(
            rule.Id, request.SensorId, request.Condition,
            request.Threshold, request.Hysteresis);
        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error);
        }

        await ruleConditionRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result.Value.Id);
    }

    private async Task<Result> EnsureConditionOwnsSensorAsync(
        Domain.Entities.AutomationRule rule,
        Guid sensorId,
        CancellationToken cancellationToken)
    {
        SensorEntity? sensor = await sensorRepository.GetByIdAsync(sensorId, cancellationToken);
        if (sensor is null)
        {
            return Result.Failure(Error.NotFound("Sensor.NotFound", "Sensor not found"));
        }

        if (sensor.EcosystemId != rule.EcosystemId)
        {
            return Result.Failure(Error.Validation(
                    "Condition.InvalidSensor", "Sensor must belong to the same ecosystem as the rule"));
        }

        return Result.Success();
    }
}
