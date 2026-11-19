using System.Globalization;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;

namespace Billing.Domain.ValueObjects;

public sealed record BillingMoney : Money
{
    public const int CurrencyLength = 3;

    public string Currency { get; }

    private BillingMoney(decimal amount, string currency) : base(amount)
    {
        Currency = currency;
    }

    public static BillingMoney Parse(decimal amount, string currency) =>
        new(amount, currency.Trim().ToUpperInvariant());

    public static Result<BillingMoney> Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            return Result<BillingMoney>.Failure(Error.Validation<BillingMoney>(
                "Amount cannot be negative."));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            return Result<BillingMoney>.Failure(Error.Validation<BillingMoney>(
                "Currency is required."));
        }

        string normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length is not CurrencyLength ||
           !normalizedCurrency.All(char.IsAsciiLetter))
        {
            return Result<BillingMoney>.Failure(Error.Validation<BillingMoney>(
                "Currency must be a valid 3-letter ISO 4217 code."));
        }

        return Result<BillingMoney>.Success(new BillingMoney(amount, normalizedCurrency));
    }

    public static Result<BillingMoney> Zero(string currency) => Create(0m, currency);

    public Result<BillingMoney> Add(BillingMoney other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            return Result<BillingMoney>.Failure(Error.Validation<BillingMoney>(
                $"Cannot add amounts with different currencies: {Currency} and {other.Currency}."));
        }

        return Result<BillingMoney>.Success(new BillingMoney(Amount + other.Amount, Currency));
    }

    public Result<BillingMoney> Subtract(BillingMoney other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            return Result<BillingMoney>.Failure(Error.Validation<BillingMoney>(
                $"Cannot subtract amounts with different currencies: {Currency} and {other.Currency}."));
        }

        decimal result = Amount - other.Amount;
        if (result < 0)
        {
            return Result<BillingMoney>.Failure(Error.Validation<BillingMoney>(
                "Resulting amount cannot be negative."));
        }

        return Result<BillingMoney>.Success(new BillingMoney(result, Currency));
    }

    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture, "{0:F2} {1}", Amount, Currency);
}
