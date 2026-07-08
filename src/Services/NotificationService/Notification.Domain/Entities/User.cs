// Ignore Spelling: tg

using Contracts.Abstractions;
using Contracts.Results;
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
        Result<PhoneNumber> phoneResult = PhoneNumber.Create(rawPhoneNumber);
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

        if (tgEnable && telegramChatId is null)
        {
            return Result<User>.Failure(Error.Validation<User>(
                "Telegram chat id must not be empty if tgEnable is true."));
        }

        var user = new User(
            userId, emailResult.Value, tzResult.Value, phoneResult.Value,
            emailEnable, tgEnable, telegramChatId,
            isNotifyEnabled: enable,
            createdAt);

        return Result<User>.Success(user);
    }

    public Result UpdateContacts(
        string email,
        string phoneNumber)
    {
        Result<PhoneNumber> phoneResult = PhoneNumber.Create(phoneNumber);
        if (phoneResult.IsFailure)
        {
            return Result<User>.Failure(phoneResult.Error);
        }

        Result<EmailAddress> emailResult = EmailAddress.Create(email);
        if (emailResult.IsFailure)
        {
            return Result<User>.Failure(emailResult.Error);
        }

        Email = emailResult.Value;
        PhoneNumber = phoneResult.Value;

        IncrementVersion();

        return Result.Success();
    }

    public Result SetNotificationPreferences(bool emailEnable, bool tgEnable, long? tgChatId)
    {
        if (tgEnable && tgChatId is null)
        {
            return Result.Failure(Error.Validation<User>(
                "Telegram Chat ID is required if Telegram notifications are enabled."));
        }

        EmailEnable = emailEnable;
        TgEnable = tgEnable;
        TelegramChatId = tgChatId;

        IncrementVersion();

        return Result.Success();
    }
}
