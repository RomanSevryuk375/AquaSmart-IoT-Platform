using Contracts.Results;
using FluentAssertions;
using Notification.Domain.Entities;
using Notification.TestShared.Builders;

namespace Notification.Domain.UnitTests.Entities;

public class ReminderTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndInitializesProperties()
    {
        // Arrange
        var reminderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        string taskName = "Feed the fish";
        int intervalDays = 5;

        // Act
        Result<Reminder> result = Reminder.Create(reminderId, userId, ecosystemId, taskName, intervalDays);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(reminderId);
        result.Value.UserId.Should().Be(userId);
        result.Value.EcosystemId.Should().Be(ecosystemId);
        result.Value.TaskName.Value.Should().Be(taskName);
        result.Value.IntervalDays.Should().Be(intervalDays);
        result.Value.LastDoneAt.Should().BeNull();
        result.Value.LastNotifiedAt.Should().BeNull();
        result.Value.NextDueAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(intervalDays), TimeSpan.FromSeconds(5));
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.Version.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_WithInvalidTaskName_ReturnsFailure()
    {
        // Arrange
        var reminderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        string invalidTaskName = "";

        // Act
        Result<Reminder> result = Reminder.Create(reminderId, userId, ecosystemId, invalidTaskName, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
    }

    [Fact]
    public void Create_WithNegativeIntervalDays_ReturnsFailure()
    {
        // Arrange
        var reminderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        string taskName = "Feed the fish";
        int invalidIntervalDays = -1;

        // Act
        Result<Reminder> result = Reminder.Create(reminderId, userId, ecosystemId, taskName, invalidIntervalDays);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reminder.Invalid");
        result.Error.Message.Should().Be("Interval days must be positive.");
    }

    [Fact]
    public void MarkAsNotified_SetsLastNotifiedAtAndIncrementsVersion()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder().Build();
        Guid initialVersion = reminder.Version;

        // Act
        reminder.MarkAsNotified();

        // Assert
        reminder.LastNotifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        reminder.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void CompleteTask_UpdatesDatesAndIncrementsVersion()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder().WithIntervalDays(5).Build();
        Guid initialVersion = reminder.Version;

        // Act
        reminder.CompleteTask();

        // Assert
        reminder.LastDoneAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        reminder.NextDueAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(5), TimeSpan.FromSeconds(5));
        reminder.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void UpdateSchedule_WithValidDataAndNoLastDone_UpdatesScheduleAndIncrementsVersion()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder()
            .WithIntervalDays(2)
            .Build();
        Guid initialVersion = reminder.Version;
        string newTaskName = "Clean the skimmer";
        int newIntervalDays = 10;

        // Act
        Result result = reminder.UpdateSchedule(newTaskName, newIntervalDays);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reminder.TaskName.Value.Should().Be(newTaskName);
        reminder.IntervalDays.Should().Be(newIntervalDays);
        reminder.NextDueAt.Should().BeCloseTo(reminder.CreatedAt.AddDays(newIntervalDays), TimeSpan.FromSeconds(5));
        reminder.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void UpdateSchedule_WithValidDataAndLastDone_UpdatesScheduleAndIncrementsVersion()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder()
            .WithIntervalDays(2)
            .WithIsCompleted(true)
            .Build();
        Guid initialVersion = reminder.Version;
        string newTaskName = "Clean the skimmer";
        int newIntervalDays = 10;
        DateTime? lastDone = reminder.LastDoneAt;

        // Act
        Result result = reminder.UpdateSchedule(newTaskName, newIntervalDays);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reminder.TaskName.Value.Should().Be(newTaskName);
        reminder.IntervalDays.Should().Be(newIntervalDays);
        reminder.NextDueAt.Should().BeCloseTo(lastDone!.Value.AddDays(newIntervalDays), TimeSpan.FromSeconds(5));
        reminder.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void UpdateSchedule_WithInvalidTaskName_ReturnsFailureAndDoesNotChangeVersion()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder().Build();
        Guid initialVersion = reminder.Version;

        // Act
        Result result = reminder.UpdateSchedule("", 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        reminder.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void UpdateSchedule_WithNegativeIntervalDays_ReturnsFailureAndDoesNotChangeVersion()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder().Build();
        Guid initialVersion = reminder.Version;

        // Act
        Result result = reminder.UpdateSchedule("Feed the fish", -5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reminder.Invalid");
        result.Error.Message.Should().Be("Interval days must be positive.");
        reminder.Version.Should().Be(initialVersion);
    }
}
