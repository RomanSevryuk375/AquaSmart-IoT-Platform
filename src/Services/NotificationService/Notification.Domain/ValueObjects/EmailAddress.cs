using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace Notification.Domain.ValueObjects;

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
                "Email address is required."));
        }

        string cleanEmail = value.Trim().ToLowerInvariant();
        if (cleanEmail.Length > 256)
        {
            return Result<EmailAddress>.Failure(Error.Validation<EmailAddress>(
                "Email address cannot exceed 256 characters."));
        }

        if (!EmailRegex().IsMatch(cleanEmail))
        {
            return Result<EmailAddress>.Failure(Error.Validation<EmailAddress>(
                "Invalid email address format."));
        }

        return Result<EmailAddress>.Success(new EmailAddress(cleanEmail));
    }

    [GeneratedRegex(IdentityConstants.EmailNumberRegex, RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    public override string ToString() => Value;
}
