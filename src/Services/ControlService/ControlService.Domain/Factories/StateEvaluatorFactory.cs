using Contracts.Enums;

namespace Control.Domain.Factories;

public static class StateEvaluatorFactory
{
    public static bool EvaluateEnum(RuleAction action)
    {
        return action switch
        {
            RuleAction.SwitchOn => true,
            RuleAction.SwitchOff => false,

            _ => false,
        };
    }

    public static RuleAction EvaluateBool(bool value)
    {
        return value switch
        {
            true => RuleAction.SwitchOn,
            false => RuleAction.SwitchOff,
        };
    }
}
