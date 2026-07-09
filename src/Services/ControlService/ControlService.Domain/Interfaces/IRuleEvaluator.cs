namespace Control.Domain.Interfaces;

public interface IRuleEvaluator
{
    public bool? Evaluate(
        double currentValue,
        double threshold,
        double hysteresis);
}
