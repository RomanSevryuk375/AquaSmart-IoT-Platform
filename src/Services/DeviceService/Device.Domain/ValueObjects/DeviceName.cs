using Contracts.Constants;
using Contracts.Results;

namespace Device.Domain.ValueObjects;

public sealed record DeviceName
{
    public string Value { get; } = string.Empty;

    private DeviceName(string name)
    {
        Value = name;   
    }

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
