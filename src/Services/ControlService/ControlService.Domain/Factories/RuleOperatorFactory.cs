using Contracts.Enums;

namespace Control.Domain.Factories;

public static class RuleOperatorFactory
{
    public static bool? CalculateTargetState(
        List<bool?> results,
        Operator @operator,
        RuleAction action)
    {
        if (results.Count == 0)
        {
            return false;
        }

        bool? isRuleTriggered = @operator switch
        {
            Operator.AND => EvaluateAnd(results),
            Operator.OR => EvaluateOr(results),
            Operator.NOT => EvaluateNot(results),
            Operator.XOR => EvaluateXor(results),

            _ => throw new ArgumentOutOfRangeException(nameof(@operator))
        };

        if (isRuleTriggered == null)
        {
            return null;
        }

        return isRuleTriggered.Value
            ? (action == RuleAction.SwitchOn)
            : (action == RuleAction.SwitchOff);
    }

    private static bool? EvaluateAnd(List<bool?> results)
    {
        if (results.Any(r => r == false))
        {
            return false;
        }

        if (results.All(r => r == true))
        {
            return true;
        }

        return null;
    }

    private static bool? EvaluateOr(List<bool?> results)
    {
        if (results.Any(r => r == true))
        {
            return true;
        }

        if (results.All(r => r == false))
        {
            return false;
        }

        return null;
    }

    private static bool? EvaluateNot(List<bool?> results)
    {
        bool? baseValue = results.First();
        if (baseValue == null)
        {
            return null;
        }

        return !baseValue.Value;
    }

    private static bool? EvaluateXor(List<bool?> results)
    {
        if (results.Any(r => r == null))
        {
            return null;
        }

        return results.Count(r => r == true) % 2 != 0;
    }
}
