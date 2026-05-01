using Contracts.Enums;

namespace Control.Domain.Factories;

public static class RuleOperatorFactory
{
    public static bool? CalculateTargetState(
        List<bool?> results,
        OperatorEnum @operator,
        RuleActionEnum action)
    {
        if (results.Count == 0)
        {
            return false;
        }

        bool? isRuleTriggered = @operator switch
        {
            OperatorEnum.AND => EvaluateAnd(results),
            OperatorEnum.OR => EvaluateOr(results),
            OperatorEnum.NOT => EvaluateNot(results),
            OperatorEnum.XOR => EvaluateXor(results),

            _ => throw new ArgumentOutOfRangeException(nameof(@operator))
        };

        if (isRuleTriggered == null)
        {
            return null;
        }

        return isRuleTriggered.Value
            ? (action == RuleActionEnum.SwitchOn)
            : (action == RuleActionEnum.SwitchOff);
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
        var baseValue = results.First();
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
