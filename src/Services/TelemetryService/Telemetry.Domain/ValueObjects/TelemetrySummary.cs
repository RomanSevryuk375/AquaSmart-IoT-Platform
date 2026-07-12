using Contracts.Constants;
using Contracts.Results;

namespace Telemetry.Domain.ValueObjects;

public sealed record TelemetrySummary
{
    public double MaxValue { get; }
    public double MinValue { get; }
    public double AvgValue { get; }
    public int Count { get; }

    private TelemetrySummary(
        double minValue,
        double avgValue,
        double maxValue,
        int count)
    {
        MinValue = minValue;
        AvgValue = avgValue;
        MaxValue = maxValue;
        Count = count;
    }

    public static Result<TelemetrySummary> Create(
        double minValue,
        double avgValue,
        double maxValue,
        int count)
    {
        var errors = new List<string>();

        if (count <= 0)
        {
            errors.Add(ErrorMessages.TelemetrySummary.DataPointsCountMustBePositive);
        }

        if (minValue > maxValue)
        {
            errors.Add(ErrorMessages.TelemetrySummary.MinValueCannotBeGreaterThanMaxValue);
        }

        if (avgValue < minValue || avgValue > maxValue)
        {
            errors.Add(ErrorMessages.TelemetrySummary.AvgValueMustBeBetweenMinAndMax);
        }

        if (errors.Count > 0)
        {
            return Result<TelemetrySummary>.Failure(Error.Validation<TelemetrySummary>(
                 string.Join("; ", errors)));
        }

        var summary = new TelemetrySummary(minValue, avgValue, maxValue, count);

        return Result<TelemetrySummary>.Success(summary);
    }
}
