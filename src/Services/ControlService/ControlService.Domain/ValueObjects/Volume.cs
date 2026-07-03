using Contracts.Results;

namespace Control.Domain.ValueObjects;

public sealed record Volume
{
    private const int MaxVolume = 10_000_000;
    private const int MinVolume = 0;

    public double Value { get; }

    private Volume(double value)
    {
        Value = value;
    }

    public static Result<Volume> Create(double value)
    {
        if (value <= MinVolume)
        {
            return Result<Volume>.Failure(Error.Validation<Volume>(
                "Volume must be strictly greater than zero."));
        }

        if (value > MaxVolume)
        {
            return Result<Volume>.Failure(Error.Validation<Volume>(
                "Volume is unrealistically large."));
        }

        return Result<Volume>.Success(new Volume(value));
    }

    public override string ToString() => Value.ToString();
}
