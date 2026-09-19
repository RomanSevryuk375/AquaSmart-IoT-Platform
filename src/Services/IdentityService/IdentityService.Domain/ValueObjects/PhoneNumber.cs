using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace IdentityService.Domain.ValueObjects;

public sealed partial record PhoneNumber
{
    public string Value { get; }

    internal PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Parse(string dbVal) => new(dbVal);

    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<PhoneNumber>.Failure(Error.Validation<PhoneNumber>(
                ErrorMessages.Identity.PhoneRequired));
        }

        string cleanNumber = value.Trim();

        if (!PhoneNumberRegex().IsMatch(cleanNumber))
        {
            return Result<PhoneNumber>.Failure(Error.Validation<PhoneNumber>(
                ErrorMessages.Identity.PhoneFormatInvalid));
        }

        return Result<PhoneNumber>.Success(new PhoneNumber(cleanNumber));
    }

    [GeneratedRegex(IdentityConstants.PhoneNumberRegex)]
    private static partial Regex PhoneNumberRegex();

    public override string ToString() => Value;
}
