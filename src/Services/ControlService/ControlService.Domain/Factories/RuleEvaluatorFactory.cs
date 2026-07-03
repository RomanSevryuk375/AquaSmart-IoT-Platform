using Contracts.Enums;
using Control.Domain.Interfaces;
using Control.Domain.Strategies;

namespace Control.Domain.Factories;

public static class RuleEvaluatorFactory
{
    public static IRuleEvaluator Create(Condition condition)
    {
        return condition switch
        {
            Condition.Equal => new EqualConditionEvaluator(),
            Condition.Greater => new GreaterConditionEvaluator(),
            Condition.GreaterOrEqual => new GreaterOrEqualConditionEvaluator(),
            Condition.Less => new LessConditionEvaluator(),
            Condition.LessOrEqual => new LessOrEqualConditionEvaluator(),

            _ => throw new ArgumentOutOfRangeException($"Condition {condition} is not supported")
        };
    }
}
