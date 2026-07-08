using Contracts.Enums;
using Contracts.Results;
using Notification.TestShared.Constants;
using DomainNotification = Notification.Domain.Entities.Notification;

namespace Notification.TestShared.Builders;

public class NotificationBuilder
{
    private Guid _id = NotificationTestConstants.NotificationId;
    private Guid _userId = NotificationTestConstants.UserId;
    private Guid? _ecosystemId = NotificationTestConstants.EcosystemId;
    private NotificationLevel _level = NotificationLevel.Info;
    private string _message = "Regular notification message";
    private bool _isRead = false;
    private bool _isPublished = false;
    private string? _failureReason = null;
    private int _retryCount = 0;

    public NotificationBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public NotificationBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public NotificationBuilder WithEcosystemId(Guid? ecosystemId)
    {
        _ecosystemId = ecosystemId;
        return this;
    }

    public NotificationBuilder WithLevel(NotificationLevel level)
    {
        _level = level;
        return this;
    }

    public NotificationBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }

    public NotificationBuilder WithIsRead(bool isRead)
    {
        _isRead = isRead;
        return this;
    }

    public NotificationBuilder WithIsPublished(bool isPublished)
    {
        _isPublished = isPublished;
        return this;
    }

    public NotificationBuilder WithFailure(string reason, int retryCount = 1)
    {
        _failureReason = reason;
        _retryCount = retryCount;
        return this;
    }

    public DomainNotification Build()
    {
        Result<DomainNotification> result = DomainNotification.Create(
            _id,
            _userId,
            _ecosystemId,
            _level,
            _message);

        if (result.IsFailure)
        {
            throw new ArgumentException($"NotificationBuilder failed: {result.Error.Message}");
        }

        DomainNotification notification = result.Value;

        if (_isRead)
        {
            notification.MarkAsRead();
        }

        if (_isPublished)
        {
            notification.MarkAsPublished();
        }

        if (_failureReason != null)
        {
            int retries = _retryCount > 0 ? _retryCount : 1;
            for (int i = 0; i < retries; i++)
            {
                notification.MarkAsFailure(_failureReason);
            }
        }

        return notification;
    }
}
