using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;

namespace Billing.Domain.ValueObjects;

public sealed record CardDetails
{
    public const int Last4Length = 4;
    public const int MaxBrandLength = 32;
    public const int MinExpYear = 2000;
    public const int MaxExpYear = 2100;

    public string CardLast4 { get; }
    public string CardBrand { get; }
    public int ExpMonth { get; }
    public int ExpYear { get; }

    private CardDetails(string cardLast4, string cardBrand, int expMonth, int expYear)
    {
        CardLast4 = cardLast4;
        CardBrand = cardBrand;
        ExpMonth = expMonth;
        ExpYear = expYear;
    }

    public static CardDetails Parse(string cardLast4, string cardBrand, int expMonth, int expYear) =>
        new(cardLast4, cardBrand, expMonth, expYear);

    public static Result<CardDetails> Create(string cardLast4, string cardBrand, int expMonth, int expYear)
    {
        var errors = new List<string>();

        string cleanLast4 = cardLast4?.Trim() ?? string.Empty;
        if (cleanLast4.Length is not Last4Length || !cleanLast4.All(char.IsAsciiDigit))
        {
            errors.Add("Card last 4 must contain exactly 4 digits.");
        }

        string cleanBrand = DomainGuard.CheckText(cardBrand, MaxBrandLength, "Card brand", errors);
        DomainGuard.CheckInRange(expMonth, 1, 12, "Card expiration month", errors);
        DomainGuard.CheckInRange(expYear, MinExpYear, MaxExpYear, "Card expiration year", errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<CardDetails>(errors);
        }

        return Result<CardDetails>.Success(new CardDetails(cleanLast4, cleanBrand, expMonth, expYear));
    }

    public bool IsExpired(DateTime utcNow) =>
        ExpYear < utcNow.Year || (ExpYear == utcNow.Year && ExpMonth < utcNow.Month);

    public override string ToString() => $"{CardBrand} **** {CardLast4} ({ExpMonth:D2}/{ExpYear})";
}
