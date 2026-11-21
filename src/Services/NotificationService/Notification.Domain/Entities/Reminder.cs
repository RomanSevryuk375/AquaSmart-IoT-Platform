using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
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
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(reminderId, "Reminder Id", errors);
        DomainGuard.CheckNotEmpty(userId, nameof(UserId), errors);
        DomainGuard.CheckNotEmpty(ecosystemId, nameof(EcosystemId), errors);
        Name? taskName = DomainGuard.Collect(Name.Create(rawTaskName), errors);
        if (intervalDays < 0)
        {
            errors.Add("Interval days must be positive.");
        }

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Reminder>(errors);
        }

        var reminder = new Reminder(
            reminderId, userId, ecosystemId,
            taskName!,
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
        var errors = new List<string>();

        Name? name = DomainGuard.Collect(Name.Create(taskName), errors);
        if (intervalDays < 0)
        {
            errors.Add("Interval days must be positive.");
        }

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation<Reminder>(string.Join(", ", errors)));
        }

        TaskName = name!;
        IntervalDays = intervalDays;
        NextDueAt = LastDoneAt.HasValue
            ? LastDoneAt.Value.AddDays(IntervalDays)
            : CreatedAt.AddDays(IntervalDays);

        IncrementVersion();

        return Result.Success();
    }
}
