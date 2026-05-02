using Contracts.Abstractions;
using System.Text.RegularExpressions;

namespace Notification.Domain.Entities;

public sealed class UserEntity : IEntity
{
    private UserEntity(
        Guid id,
        string email,
        string timeZone,
        string phoneNumber,
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

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string TimeZone { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool EmailEnable { get; private set; }
    public bool TgEnable { get; private set; }
    public long? TelegramChatId { get; private set; }
    public bool IsNotifyEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static (UserEntity? user, List<string> errors) Create(
        Guid id,
        string email,
        string timeZone,
        string phoneNumber,
        bool emailEnable,
        bool tgEnable,
        long? telegramChatId,
        bool enable,
        DateTime createdAt)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("id must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("email must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(timeZone))
        {
            errors.Add("timeZone must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            errors.Add("phonenumber must not be empty.");
        }

        if (!Regex.IsMatch(phoneNumber, @"^(\+375|80)(29|44|33|25)\d{7}$"))
        {
            errors.Add("Phone number should be in format +375XXXXXXXXX or 80XXXXXXXXX");
        }

        if (tgEnable is true && telegramChatId is null)
        {
            errors.Add("telegramChatId must not be empty if tgEnable is true.");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var user = new UserEntity(
            id, 
            email,
            timeZone,
            phoneNumber,
            emailEnable,
            tgEnable,
            telegramChatId,
            enable,
            createdAt);

        return (user, errors);
    }

    public List<string>? UpdateContacts(
        string email, 
        string phoneNumber)
    {
        Email = email.Trim();

        var errors = new List<string>();

        if (!Regex.IsMatch(phoneNumber, @"^(\+375|80)(29|44|33|25)\d{7}$"))
        {
            errors.Add("Phone number should be in format +375XXXXXXXXX or 80XXXXXXXXX");
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        PhoneNumber = phoneNumber.Trim();

        return null;
    }

    public void SetNotificationPreferences(
        bool emailEnable, 
        bool tgEnable, 
        long? tgChatId)
    {
        EmailEnable = emailEnable;
        TgEnable = tgEnable;
        TelegramChatId = tgChatId;
    }
}
