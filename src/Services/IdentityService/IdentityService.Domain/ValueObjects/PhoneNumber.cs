using System.Text.RegularExpressions;
using Contracts.Constants;
using Contracts.Results;

namespace IdentityService.Domain.ValueObjects;

public sealed partial record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<PhoneNumber>.Failure(Error.Validation<PhoneNumber>(
                "Phone number is required."));
        }

        string cleanNumber = value.Trim();

        if (!PhoneNumberRegex().IsMatch(cleanNumber))
        {
            return Result<PhoneNumber>.Failure(Error.Validation<PhoneNumber>(
                "Phone number should be in format +375XXXXXXXXX or 80XXXXXXXXX."));
        }

        return Result<PhoneNumber>.Success(new PhoneNumber(cleanNumber));
    }

    [GeneratedRegex(IdentityConstants.PhoneNumberRegex)]
    private static partial Regex PhoneNumberRegex();

    public override string ToString() => Value;
}
