using Control.Domain.Interfaces;

namespace Control.Domain.Strategies;

public sealed class LessOrEqualConditionEvaluator : IRuleEvaluator
{
    public bool? Evaluate(double currentValue, double threshold, double hysteresis)
    {
        if (currentValue <= threshold - hysteresis)
        {
            return true;
        }

        if (currentValue >= threshold + hysteresis)
        {
            return false;
        }

        return null;
    }
}
