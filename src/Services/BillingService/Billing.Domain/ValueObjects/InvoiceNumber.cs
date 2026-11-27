using System.Globalization;
using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;

namespace Billing.Domain.ValueObjects;

public sealed partial record InvoiceNumber
{
    public const int MaxLength = 64;
    private const string Pattern = @"^INV-\d{4}-\d{5,}$";

    public string Value { get; }

    private InvoiceNumber(string value)
    {
        Value = value;
    }

    public static InvoiceNumber Parse(string dbVal) => new(dbVal);

    public static Result<InvoiceNumber> Create(string value)
    {
        Result<string> textResult = DomainGuard.RequireText<InvoiceNumber>(value, MaxLength, "Invoice number");
        if (textResult.IsFailure)
        {
            return Result<InvoiceNumber>.Failure(textResult.Error);
        }

        string normalized = textResult.Value.ToUpperInvariant();
        if (!InvoiceNumberRegex().IsMatch(normalized))
        {
            return Result<InvoiceNumber>.Failure(Error.Validation<InvoiceNumber>(
                "Invoice number must follow the format INV-YYYY-NNNNN."));
        }

        return Result<InvoiceNumber>.Success(new InvoiceNumber(normalized));
    }

    public static Result<InvoiceNumber> Generate(int year, long sequenceNumber)
    {
        var errors = new List<string>();

        DomainGuard.CheckInRange(year, 2000, 2100, "Invoice year", errors);
        DomainGuard.CheckPositive(sequenceNumber, "Invoice sequence number", errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<InvoiceNumber>(errors);
        }

        string formatted = string.Format(CultureInfo.InvariantCulture, "INV-{0:D4}-{1:D5}", year, sequenceNumber);
        return Result<InvoiceNumber>.Success(new InvoiceNumber(formatted));
    }

    [GeneratedRegex(Pattern, RegexOptions.CultureInvariant)]
    private static partial Regex InvoiceNumberRegex();

    public override string ToString() => Value;
}
