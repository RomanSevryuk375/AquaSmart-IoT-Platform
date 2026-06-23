using Contracts.Enums;
using Control.Domain.Interfaces;
using Control.Domain.Strategies;

namespace Control.Domain.Factories;

public static class RuleEvaluatorFactory
{
    public static IRuleEvaluator Create (RuleCondition condition)
    {
        return condition switch
        {
            RuleCondition.Equal => new EqualConditionEvaluator(),
            RuleCondition.Greater => new GreaterConditionEvaluator(),
            RuleCondition.GreaterOrEqual => new GreaterOrEqualConditionEvaluator(),
            RuleCondition.Less => new LessConditionEvaluator(),
            RuleCondition.LessOrEqual => new LessOrEqualConditionEvaluator(),

            _ => throw new ArgumentOutOfRangeException($"Condition {condition} is not supported")
        };
    }
}
