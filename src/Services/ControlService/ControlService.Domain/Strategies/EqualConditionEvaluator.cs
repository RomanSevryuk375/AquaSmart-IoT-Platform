using Control.Domain.Interfaces;

namespace Control.Domain.Strategies;

public sealed class EqualConditionEvaluator : IRuleEvaluator
{
    public bool? Evaluate(double currentValue, double threshold, double hysteresis)
        => currentValue >= (threshold - hysteresis) && currentValue <= (threshold + hysteresis);
}
