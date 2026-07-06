using System.Data;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Behaviors;

internal class ConditionOwnsSensorBehavior<TRequest, TResponse>(
    ISensorRepository sensorRepository,
    IAutomationRuleRepository ruleRepository)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IRuleSensorBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        AutomationRule? rule = await ruleRepository.GetByIdAsync(request.RuleId, cancellationToken);
        if (rule is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Rule>(
                $"Rule {request.RuleId} not found"));
        }

        Sensor? sensor = await sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
        if (sensor is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Sensor>(
                "Sensor not found"));
        }

        if (sensor.EcosystemId != rule.EcosystemId)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.Validation<RuleCondition>(
                "Sensor must belong to the same ecosystem as the rule"));
        }

        return await next();
    }
}
