using Contracts.Abstractions;
using Contracts.Enums;

namespace Notification.Domain.Entities;

public sealed class NotificationEntity : IEntity
{
    private NotificationEntity(
        Guid id, 
        Guid userId, 
        Guid? ecosystemId, 
        NotificationLevel level, 
        string message, 
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

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? EcosystemId { get; private set; }
    public NotificationLevel Level { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? FailureReason { get; private set; }


    public static (NotificationEntity? notification, List<string> errors) Create(
        Guid userId, 
        Guid? ecosystemId, 
        NotificationLevel level, 
        string message)
    {
        var errors = new List<string>();

        if (userId == Guid.Empty)
        {
            errors.Add("userId must not be empty.");
        }

        if (ecosystemId == Guid.Empty)
        {
            errors.Add("aquariumId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            errors.Add("message must not be empty.");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var notification = new NotificationEntity(
            Guid.NewGuid(),
            userId,
            ecosystemId,
            level,
            message,
            false,
            DateTime.UtcNow,
            false,
            null,
            0,
            null);

        return (notification, errors);
    }

    public void MarkAsFailure(string exception)
    {
        IsPublished = false;
        RetryCount++;
        FailureReason = exception;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }

    public void MarkAsPublished()
    {
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
    }
}