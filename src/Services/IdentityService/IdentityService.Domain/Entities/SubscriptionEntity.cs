using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;
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
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "Subscription Id", errors);
        Name? name = DomainGuard.Collect(Name.Create(rawName), errors);
        Money? price = DomainGuard.Collect(Money.Create(rawPrice), errors);
        DomainGuard.CheckNonNegative(durationDays, "Duration", errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Subscription>(errors);
        }

        return Result<Subscription>.Success(new Subscription(
            id, name!, price!, durationDays, permissions));
    }
}
