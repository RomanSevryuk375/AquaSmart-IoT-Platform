using Contracts.Constants;
using Contracts.Results;

namespace Control.Domain.ValueObjects;

public sealed record Volume
{
    public double Value { get; }

    private Volume(double value)
    {
        Value = value;
    }

    public static Result<Volume> Create(double value)
    {
        if (value <= ControlConstants.MinVolume)
        {
            return Result<Volume>.Failure(Error.Validation<Volume>(
                ControlValidationMessages.VolumeMustBeGreaterThanZero));
        }

        if (value > ControlConstants.MaxVolume)
        {
            return Result<Volume>.Failure(Error.Validation<Volume>(
                ControlValidationMessages.VolumeIsUnrealisticallyLarge));
        }

        return Result<Volume>.Success(new Volume(value));
    }

    public override string ToString() => Value.ToString();
}
