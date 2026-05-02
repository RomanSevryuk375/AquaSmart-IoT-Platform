using Contracts.Enums;

namespace Control.Domain.Factories;

public static class StateEvaluatorFactory
{
    public static bool EvaluateEnum(RuleActionEnum action)
    {
        return action switch
        {
            RuleActionEnum.SwitchOn => true,
            RuleActionEnum.SwitchOff => false,

            _ => false,
        };
    }

    public static RuleActionEnum EvaluateBool(bool value)
    {
        return value switch
        {
            true => RuleActionEnum.SwitchOn,
            false => RuleActionEnum.SwitchOff,
        };
    }
}
