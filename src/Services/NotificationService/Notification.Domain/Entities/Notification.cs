using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.Entities;

public sealed class Notification : AggregateRoot, IEntity
{
    private Notification(
        Guid id,
        Guid userId,
        Guid? ecosystemId,
        NotificationLevel level,
        MessageText message,
        bool isRead,
        DateTime createdAt,

        bool isPublished,
        DateTime? publishedAt,
        int retryCount,
        string? failureReason)
    {
        Id = id;
        UserId = userId;
        EcosystemId = ecosystemId;
        Level = level;
        Message = message;
        IsRead = isRead;
        CreatedAt = createdAt;

        IsPublished = isPublished;
        PublishedAt = publishedAt;
        RetryCount = retryCount;
        FailureReason = failureReason;
    }

#pragma warning disable CS8618
    private Notification() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? EcosystemId { get; private set; }
    public NotificationLevel Level { get; private set; }
    public MessageText Message { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? FailureReason { get; private set; }


    public static Result<Notification> Create(
        Guid notificationId,
        Guid userId,
        Guid? ecosystemId,
        NotificationLevel level,
        string rawMessage)
    {
        Result<MessageText> messageResult = MessageText.Create(rawMessage);
        if (messageResult.IsFailure)
        {
            return Result<Notification>.Failure(messageResult.Error);
        }

        var notification = new Notification(
            notificationId, userId, ecosystemId,
            level, messageResult.Value, isRead: false,
            createdAt: DateTime.UtcNow,

            isPublished: false, publishedAt: null,
            retryCount: 0, failureReason: null);

        return Result<Notification>.Success(notification);
    }

    public void MarkAsPublished()
    {
        if (IsPublished)
        {
            return;
        }

        IsPublished = true;
        PublishedAt = DateTime.UtcNow;

        IncrementVersion();
    }

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;

        IncrementVersion();
    }

    public void MarkAsFailure(string exception)
    {
        IsPublished = false;
        RetryCount++;
        FailureReason = exception;

        IncrementVersion();
    }
}
