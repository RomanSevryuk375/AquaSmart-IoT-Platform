// Ignore Spelling: tg

using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.Entities;

public sealed class User : AggregateRoot, IEntity
{
    private User(
        Guid id,
        EmailAddress email,
        TimeZoneId timeZone,
        PhoneNumber phoneNumber,
        bool emailEnable,
        bool tgEnable,
        long? telegramChatId,
        bool isNotifyEnabled,
        DateTime createdAt)
    {
        Id = id;
        Email = email;
        TimeZone = timeZone;
        PhoneNumber = phoneNumber;
        EmailEnable = emailEnable;
        TgEnable = tgEnable;
        TelegramChatId = telegramChatId;
        IsNotifyEnabled = isNotifyEnabled;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private User() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public EmailAddress Email { get; private set; }
    public TimeZoneId TimeZone { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public bool EmailEnable { get; private set; }
    public bool TgEnable { get; private set; }
    public long? TelegramChatId { get; private set; }
    public bool IsNotifyEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<User> Create(
        Guid userId,
        string rawEmail,
        string rawTimeZone,
        string rawPhoneNumber,
        bool emailEnable,
        bool tgEnable,
        long? telegramChatId,
        bool enable,
        DateTime createdAt)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(userId, "User Id", errors);
        PhoneNumber? phoneNumber = DomainGuard.Collect(PhoneNumber.Create(rawPhoneNumber), errors);
        TimeZoneId? timeZone = DomainGuard.Collect(TimeZoneId.Create(rawTimeZone), errors);
        EmailAddress? email = DomainGuard.Collect(EmailAddress.Create(rawEmail), errors);

        if (tgEnable && telegramChatId is null)
        {
            errors.Add("Telegram chat id must not be empty if tgEnable is true.");
        }

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<User>(errors);
        }

        var user = new User(
            userId, email!, timeZone!, phoneNumber!,
            emailEnable, tgEnable, telegramChatId,
            isNotifyEnabled: enable,
            createdAt);

        return Result<User>.Success(user);
    }

    public Result UpdateContacts(
        string email,
        string phoneNumber)
    {
        var errors = new List<string>();

        PhoneNumber? phoneVo = DomainGuard.Collect(PhoneNumber.Create(phoneNumber), errors);
        EmailAddress? emailVo = DomainGuard.Collect(EmailAddress.Create(email), errors);

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation<User>(string.Join(", ", errors)));
        }

        Email = emailVo!;
        PhoneNumber = phoneVo!;

        IncrementVersion();

        return Result.Success();
    }

    public void LinkTelegramChat(long chatId)
    {
        if (TelegramChatId == chatId)
        {
            return;
        }

        TgEnable = true;
        TelegramChatId = chatId;

        IncrementVersion();
    }
}
