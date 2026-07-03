using Contracts.Results;

namespace Control.Domain.ValueObjects;

public sealed record ConditionThreshold
{
    public double Threshold { get; }
    public double Hysteresis { get; }

    private ConditionThreshold(double threshold, double hysteresis)
    {
        Threshold = threshold;
        Hysteresis = hysteresis;
    }

    public static Result<ConditionThreshold> Create(double threshold, double hysteresis)
    {
        if (hysteresis < 0)
        {
            return Result<ConditionThreshold>.Failure(Error.Validation<ConditionThreshold>(
                "Hysteresis cannot be negative."));
        }

        return Result<ConditionThreshold>.Success(new ConditionThreshold(
            threshold, hysteresis));
    }
    public static ConditionThreshold Parse(string dbValue)
    {
        string[] parts = dbValue.Split("_", 2);
        double threshold = Convert.ToDouble(parts[0]);
        double hysteresis = Convert.ToDouble(parts[1]);

        return Create(threshold, hysteresis).Value;
    }

    public override string ToString() => $"{Threshold}_{Hysteresis}";
}
