namespace Control.Domain.Interfaces;

public interface IRuleEvaluator
{
    bool? Evaluate(
        double currentValue, 
        double threshold, 
        double hysteresis);
}
