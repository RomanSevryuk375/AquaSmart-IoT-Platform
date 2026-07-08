// Ignore Spelling: tg

using Contracts.Results;
using Notification.Domain.Entities;
using Notification.TestShared.Constants;

namespace Notification.TestShared.Builders;

public class UserBuilder
{
    private Guid _id = NotificationTestConstants.UserId;
    private string _email = "user@test.com";
    private string _timeZone = "UTC";
    private string _phoneNumber = "+375291112233";
    private bool _emailEnable = true;
    private bool _tgEnable = false;
    private long? _telegramChatId = null;
    private bool _enable = true;
    private DateTime _createdAt = DateTime.UtcNow;

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithTimeZone(string timeZone)
    {
        _timeZone = timeZone;
        return this;
    }

    public UserBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public UserBuilder WithEmailEnable(bool emailEnable)
    {
        _emailEnable = emailEnable;
        return this;
    }

    public UserBuilder WithTgEnable(bool tgEnable, long? telegramChatId = null)
    {
        _tgEnable = tgEnable;
        _telegramChatId = telegramChatId;
        return this;
    }

    public UserBuilder WithEnable(bool enable)
    {
        _enable = enable;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public User Build()
    {
        Result<User> result = User.Create(
            _id,
            _email,
            _timeZone,
            _phoneNumber,
            _emailEnable,
            _tgEnable,
            _telegramChatId,
            _enable,
            _createdAt);

        if (result.IsFailure)
        {
            throw new ArgumentException($"UserBuilder failed: {result.Error.Message}");
        }

        return result.Value;
    }
}
