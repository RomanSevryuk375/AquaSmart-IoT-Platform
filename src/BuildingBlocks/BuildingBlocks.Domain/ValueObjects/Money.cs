using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }

    protected Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money Parse(decimal dbVal) => new(dbVal);

    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
        {
            return Result<Money>.Failure(Error.Validation<Money>(
                "Amount cannot be negative."));
        }

        return Result<Money>.Success(new Money(amount));
    }
}
