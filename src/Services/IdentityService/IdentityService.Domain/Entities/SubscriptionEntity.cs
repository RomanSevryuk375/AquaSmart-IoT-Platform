using Contracts.Abstractions;
using Contracts.Results;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Entities;

public sealed class Subscription : AggregateRoot, IEntity
{
    private Subscription(
        Guid id,
        Name name,
        Money price,
        int durationDays,
        List<string> permissions)
    {
        Id = id;
        Name = name;
        Price = price;
        DurationDays = durationDays;
        Permissions = permissions;
        CreatedAt = DateTime.UtcNow;
    }

#pragma warning disable CS8618
    private Subscription() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Name Name { get; private set; }
    public Money Price { get; private set; }
    public int DurationDays { get; private set; }
    public IReadOnlyList<string> Permissions { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Subscription> Create(
        Guid id,
        string rawName,
        decimal rawPrice,
        int durationDays,
        List<string> permissions)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result<Subscription>.Failure(nameResult.Error);
        }

        Result<Money> priceResult = Money.Create(rawPrice);
        if (priceResult.IsFailure)
        {
            return Result<Subscription>.Failure(priceResult.Error);
        }

        if (durationDays < 0)
        {
            return Result<Subscription>.Failure(Error.Validation<Subscription>(
                "Duration cannot be negative."));
        }

        return Result<Subscription>.Success(new Subscription(
            id, nameResult.Value, priceResult.Value, durationDays, permissions));
    }
}
