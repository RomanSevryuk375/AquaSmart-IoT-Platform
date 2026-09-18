using BuildingBlocks.Domain.Results;

namespace IdentityService.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }

    internal Money(decimal amount)
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
