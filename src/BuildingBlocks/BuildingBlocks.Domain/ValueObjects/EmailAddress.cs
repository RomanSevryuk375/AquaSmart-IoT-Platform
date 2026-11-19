using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace IdentityService.Domain.ValueObjects;

public sealed partial record EmailAddress
{
    public string Value { get; }

    internal EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Parse(string dbVal) => new(dbVal);

    public static Result<EmailAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<EmailAddress>.Failure(Error.Validation<EmailAddress>(
                ErrorMessages.Identity.EmailRequired));
        }

        string cleanEmail = value.Trim().ToLowerInvariant();
        if (cleanEmail.Length > IdentityConstants.EmailLength)
        {
            return Result<EmailAddress>.Failure(Error.Validation<EmailAddress>(
                ErrorMessages.Identity.EmailTooLong));
        }

        if (!EmailRegex().IsMatch(cleanEmail))
        {
            return Result<EmailAddress>.Failure(Error.Validation<EmailAddress>(
                ErrorMessages.Identity.EmailFormatInvalid));
        }

        return Result<EmailAddress>.Success(new EmailAddress(cleanEmail));
    }

    [GeneratedRegex(IdentityConstants.EmailNumberRegex, RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    public override string ToString() => Value;
}
