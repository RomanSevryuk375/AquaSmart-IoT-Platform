using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
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
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(notificationId, "Notification Id", errors);
        DomainGuard.CheckNotEmpty(userId, nameof(UserId), errors);
        DomainGuard.CheckEnum(level, nameof(Level), errors);
        MessageText? message = DomainGuard.Collect(MessageText.Create(rawMessage), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Notification>(errors);
        }

        var notification = new Notification(
            notificationId, userId, ecosystemId,
            level, message!, isRead: false,
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
