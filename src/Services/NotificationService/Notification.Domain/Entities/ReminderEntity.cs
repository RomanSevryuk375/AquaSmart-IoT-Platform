using Contracts.Abstractions;

namespace Notification.Domain.Entities;

public sealed class ReminderEntity : IEntity
{
    private ReminderEntity(
        Guid id, 
        Guid userId, 
        Guid ecosystemId, 
        string taskName, 
        int intervalDays, 
        DateTime nextDueAt, 
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        EcosystemId = ecosystemId;
        TaskName = taskName;
        IntervalDays = intervalDays;
        NextDueAt = nextDueAt;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public string TaskName { get; private set; } = string.Empty;
    public int IntervalDays { get; private set; }
    public DateTime? LastDoneAt { get; private set; }
    public DateTime? LastNotifiedAt { get; private set; }
    public DateTime NextDueAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static (ReminderEntity? reminder, List<string> errors) Create(
        Guid userId, 
        Guid ecosystemId, 
        string taskName, 
        int intervalDays)
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

        if (string.IsNullOrWhiteSpace(taskName))
        {
            errors.Add("taskName must not be empty.");
        }

        if (intervalDays < 0)
        {
            errors.Add("intervalDays must be positive.");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var reminder = new ReminderEntity(
            Guid.NewGuid(),
            userId,
            ecosystemId,
            taskName,
            intervalDays,
            DateTime.UtcNow.AddDays(intervalDays),
            DateTime.UtcNow);

        return (reminder, errors);
    }

    public void MarkAsNotified()
    {
        LastNotifiedAt = DateTime.UtcNow;
    }

    public void CompleteTask()
    {
        LastDoneAt = DateTime.UtcNow;
        NextDueAt = DateTime.UtcNow.AddDays(IntervalDays);
    }

    public void UpdateSchedule(string taskName, int intervalDays)
    {
        TaskName = taskName.Trim();
        IntervalDays = intervalDays;

        if (LastDoneAt is null)
        {
            NextDueAt = CreatedAt.AddDays(IntervalDays);
        }
        else
        {
            NextDueAt = LastDoneAt.Value.AddDays(IntervalDays);
        }
    }
}