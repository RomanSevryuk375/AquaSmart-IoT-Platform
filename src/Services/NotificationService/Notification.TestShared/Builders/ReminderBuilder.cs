using Contracts.Results;
using Notification.Domain.Entities;
using Notification.TestShared.Constants;

namespace Notification.TestShared.Builders;

public class ReminderBuilder
{
    private Guid _id = NotificationTestConstants.ReminderId;
    private Guid _userId = NotificationTestConstants.UserId;
    private Guid _ecosystemId = NotificationTestConstants.EcosystemId;
    private string _taskName = "Feed the fish";
    private int _intervalDays = 5;
    private bool _isNotified = false;
    private bool _isCompleted = false;

    public ReminderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ReminderBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public ReminderBuilder WithEcosystemId(Guid ecosystemId)
    {
        _ecosystemId = ecosystemId;
        return this;
    }

    public ReminderBuilder WithTaskName(string taskName)
    {
        _taskName = taskName;
        return this;
    }

    public ReminderBuilder WithIntervalDays(int intervalDays)
    {
        _intervalDays = intervalDays;
        return this;
    }

    public ReminderBuilder WithIsNotified(bool isNotified)
    {
        _isNotified = isNotified;
        return this;
    }

    public ReminderBuilder WithIsCompleted(bool isCompleted)
    {
        _isCompleted = isCompleted;
        return this;
    }

    public Reminder Build()
    {
        Result<Reminder> result = Reminder.Create(
            _id,
            _userId,
            _ecosystemId,
            _taskName,
            _intervalDays);

        if (result.IsFailure)
        {
            throw new ArgumentException($"ReminderBuilder failed: {result.Error.Message}");
        }

        Reminder reminder = result.Value;

        if (_isNotified)
        {
            reminder.MarkAsNotified();
        }

        if (_isCompleted)
        {
            reminder.CompleteTask();
        }

        return reminder;
    }
}
