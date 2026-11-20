using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;
using IdentityService.Domain.Events;
using IdentityService.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities;

public sealed class User : IdentityUser<Guid>, IEntity, IHasDomainEvents
{
    private User(
        Guid id,
        Name name,
        long? telegramChatId,
        string email,
        PhoneNumber phoneNumber,
        Guid subscriptionId,
        TimeZoneId timeZone)
    {
        Id = id;
        Name = name;
        TelegramChatId = telegramChatId;
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
    public long? TelegramChatId { get; private set; }
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
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(userId, "UserId", errors);
        DomainGuard.CheckNotEmpty(subscriptionId, nameof(SubscriptionId), errors);
        Name? name = DomainGuard.Collect(Name.Create(rawName), errors);
        PhoneNumber? phoneNumber = DomainGuard.Collect(ValueObjects.PhoneNumber.Create(rawPhoneNumber), errors);
        TimeZoneId? timeZone = DomainGuard.Collect(TimeZoneId.Create(rawTimeZone), errors);
        EmailAddress? email = DomainGuard.Collect(EmailAddress.Create(rawEmail), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<User>(errors);
        }

        var user = new User(
            userId,
            name!,
            telegramChatId: null,
            email!.Value,
            phoneNumber!,
            subscriptionId,
            timeZone!);

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
        var errors = new List<string>();

        Name? name = DomainGuard.Collect(Name.Create(rawName), errors);
        PhoneNumber? phoneNumber = DomainGuard.Collect(ValueObjects.PhoneNumber.Create(rawPhoneNumber), errors);

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation<User>(string.Join(", ", errors)));
        }

        Name = name!;
        PhoneNumber = phoneNumber!.Value;

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

        RaiseEvent(new SubscriptionDowngradedDomainEvent
        {
            UserId = Id,
            NewSubscriptionId = subscriptionId,
        });

        IncrementVersion();
    }

    public void LinkTelegramChat(long chatId)
    {
        if (TelegramChatId == chatId)
        {
            return;
        }

        TelegramChatId = chatId;

        IncrementVersion();

        RaiseEvent(new TelegramAccountLinkedDomainEvent
        {
            UserId = Id,
            TelegramChatId = chatId,
        });
    }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void RaiseEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    private void IncrementVersion() => ConcurrencyStamp = Guid.NewGuid().ToString();
}
