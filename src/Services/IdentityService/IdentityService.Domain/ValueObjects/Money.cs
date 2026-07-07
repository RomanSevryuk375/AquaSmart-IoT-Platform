using Contracts.Results;

namespace IdentityService.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

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
