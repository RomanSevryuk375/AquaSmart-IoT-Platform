using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace Telemetry.Domain.ValueObjects;

public sealed record DeviceName
{
    public string Value { get; } = string.Empty;

    internal DeviceName(string name)
    {
        Value = name;
    }

    public static DeviceName Parse(string dbVal) => new(dbVal);

    public static Result<DeviceName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<DeviceName>.Failure(Error.Validation<DeviceName>(
                CommonErrors.NameEmpty));
        }

        if (name.Length > CommonConstants.NameLength)
        {
            return Result<DeviceName>.Failure(Error.Validation<DeviceName>(
                CommonErrors.InvalidNameLength));
        }

        return Result<DeviceName>.Success(new DeviceName(name.Trim()));
    }

    public override string ToString() => Value;
}
