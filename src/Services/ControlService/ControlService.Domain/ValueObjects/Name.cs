using Contracts.Constants;
using Contracts.Results;

namespace Control.Domain.ValueObjects;

public sealed record Name
{
    public string Value { get; } = string.Empty;

    private Name(string value)
    {
        Value = value;
    }

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Name>.Failure(Error.Validation<Name>(
                ControlValidationMessages.NameCannotBeEmpty));
        }

        if (value.Length > CommonConstants.NameLength)
        {
            return Result<Name>.Failure(Error.Validation<Name>(
                string.Format(ControlValidationMessages.NameCannotExceedLimitFormat, CommonConstants.NameLength)));
        }

        return Result<Name>.Success(new Name(value.Trim()));
    }

    public override string ToString() => Value;
}
