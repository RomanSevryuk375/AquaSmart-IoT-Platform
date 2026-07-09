using Contracts.Abstractions;
using Contracts.Results;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.Entities;

public sealed class Reminder : AggregateRoot, IEntity
{
    private Reminder(
        Guid id,
        Guid userId,
        Guid ecosystemId,
        Name taskName,
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

#pragma warning disable CS8618
    private Reminder() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Name TaskName { get; private set; }
    public int IntervalDays { get; private set; }
    public DateTime? LastDoneAt { get; private set; }
    public DateTime? LastNotifiedAt { get; private set; }
    public DateTime NextDueAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Reminder> Create(
        Guid reminderId,
        Guid userId,
        Guid ecosystemId,
        string rawTaskName,
        int intervalDays)
    {
        Result<Name> nameResult = Name.Create(rawTaskName);
        if (nameResult.IsFailure)
        {
            return Result<Reminder>.Failure(nameResult.Error);
        }

        if (intervalDays < 0)
        {
            return Result<Reminder>.Failure(Error.Validation<Reminder>(
                "Interval days must be positive."));
        }

        var reminder = new Reminder(
            reminderId, userId, ecosystemId,
            nameResult.Value,
            intervalDays, nextDueAt: DateTime.UtcNow.AddDays(intervalDays),
            createdAt: DateTime.UtcNow);

        return Result<Reminder>.Success(reminder);
    }

    public void MarkAsNotified()
    {
        LastNotifiedAt = DateTime.UtcNow;

        IncrementVersion();
    }

    public void CompleteTask()
    {
        LastDoneAt = DateTime.UtcNow;
        NextDueAt = DateTime.UtcNow.AddDays(IntervalDays);

        IncrementVersion();
    }

    public Result UpdateSchedule(string taskName, int intervalDays)
    {
        Result<Name> nameResult = Name.Create(taskName);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        if (intervalDays < 0)
        {
            return Result<Reminder>.Failure(Error.Validation<Reminder>(
                "Interval days must be positive."));
        }

        TaskName = nameResult.Value;
        IntervalDays = intervalDays;
        NextDueAt = LastDoneAt.HasValue
            ? LastDoneAt.Value.AddDays(IntervalDays)
            : CreatedAt.AddDays(IntervalDays);

        IncrementVersion();

        return Result.Success();
    }
}
