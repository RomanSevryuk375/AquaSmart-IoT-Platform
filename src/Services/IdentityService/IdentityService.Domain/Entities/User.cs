using Contracts.Abstractions;
using Contracts.Results;
using IdentityService.Domain.Events;
using IdentityService.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities;

public sealed class User : IdentityUser<Guid>, IEntity, IHasDomainEvents
{
    private User(
        Guid id,
        Name name,
        string email,
        PhoneNumber phoneNumber,
        Guid subscriptionId,
        TimeZoneId timeZone)
    {
        Id = id;
        Name = name;
        Email = email;
        UserName = email;
        PhoneNumber = phoneNumber.Value;
        SubscriptionId = subscriptionId;
        TimeZone = timeZone;

        SubscriptionEndDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

#pragma warning disable CS8618
    private User() { }
#pragma warning restore CS8618

    public Name Name { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public DateTime SubscriptionEndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public TimeZoneId TimeZone { get; private set; }

    public static Result<User> Create(
        Guid userId,
        string rawName,
        string rawEmail,
        string rawPhoneNumber,
        Guid subscriptionId,
        string rawTimeZone)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result<User>.Failure(nameResult.Error);
        }

        Result<PhoneNumber> phoneResult = ValueObjects.PhoneNumber.Create(rawPhoneNumber);
        if (phoneResult.IsFailure)
        {
            return Result<User>.Failure(phoneResult.Error);
        }

        Result<TimeZoneId> tzResult = TimeZoneId.Create(rawTimeZone);
        if (tzResult.IsFailure)
        {
            return Result<User>.Failure(tzResult.Error);
        }

        Result<EmailAddress> emailResult = EmailAddress.Create(rawEmail);
        if (emailResult.IsFailure)
        {
            return Result<User>.Failure(emailResult.Error);
        }

        var user = new User(
            userId,
            nameResult.Value,
            emailResult.Value.Value,
            phoneResult.Value,
            subscriptionId,
            tzResult.Value);

        user.RaiseEvent(new UserCreatedDomainEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            CreatedAt = user.CreatedAt,
            TimeZone = user.TimeZone.Value,
        });

        return Result<User>.Success(user);
    }

    public Result UpdateProfile(string rawName, string rawPhoneNumber)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        Result<PhoneNumber> phoneResult = ValueObjects.PhoneNumber.Create(rawPhoneNumber);
        if (phoneResult.IsFailure)
        {
            return Result.Failure(phoneResult.Error);
        }

        Name = nameResult.Value;
        PhoneNumber = phoneResult.Value.Value;

        IncrementVersion();

        RaiseEvent(new UserUpdatedDomainEvent
        {
            UserId = Id,
            Email = Email!,
            Name = Name.Value,
            PhoneNumber = PhoneNumber,
        });

        return Result.Success();
    }

    public void SetSubscription(Guid subscriptionId, int durationDays)
    {
        SubscriptionId = subscriptionId;
        SubscriptionEndDate = DateTime.UtcNow.AddDays(durationDays);

        IncrementVersion();
    }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void RaiseEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    private void IncrementVersion() => ConcurrencyStamp = Guid.NewGuid().ToString();
}
