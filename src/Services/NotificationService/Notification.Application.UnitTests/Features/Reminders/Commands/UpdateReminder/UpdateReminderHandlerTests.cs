using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.Reminders.Commands.UpdateReminder;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.Reminders.Commands.UpdateReminder;

public class UpdateReminderHandlerTests
{
    private readonly IReminderRepository _reminderRepoMock = Substitute.For<IReminderRepository>();
    private readonly UpdateReminderHandler _handler;

    public UpdateReminderHandlerTests()
    {
        _handler = new UpdateReminderHandler(_reminderRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenReminderExistsAndRequestIsValid_UpdatesScheduleAndReturnsSuccess()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder().Build();
        _reminderRepoMock.GetByIdAsync(reminder.Id, Arg.Any<CancellationToken>())
            .Returns(reminder);

        var command = new UpdateReminderCommand
        {
            ReminderId = reminder.Id,
            TaskName = "Clean the filters",
            IntervalDays = 10
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reminder.TaskName.Value.Should().Be("Clean the filters");
        reminder.IntervalDays.Should().Be(10);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenReminderDoesNotExist_ThrowsNullReferenceException()
    {
        // Arrange
        var nonExistentReminderId = Guid.NewGuid();
        _reminderRepoMock.GetByIdAsync(nonExistentReminderId, Arg.Any<CancellationToken>())
            .Returns((Reminder?)null);

        var command = new UpdateReminderCommand
        {
            ReminderId = nonExistentReminderId,
            TaskName = "Test",
            IntervalDays = 5
        };

        // Act
        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithInvalidRequest_ReturnsFailureResult()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder().Build();
        _reminderRepoMock.GetByIdAsync(reminder.Id, Arg.Any<CancellationToken>())
            .Returns(reminder);

        var command = new UpdateReminderCommand
        {
            ReminderId = reminder.Id,
            TaskName = "Invalid",
            IntervalDays = -5
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
