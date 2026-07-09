using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.Reminders.Commands.CompleteReminder;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.Reminders.Commands.CompleteReminder;

public class CompleteReminderHandlerTests
{
    private readonly IReminderRepository _reminderRepoMock = Substitute.For<IReminderRepository>();
    private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
    private readonly CompleteReminderHandler _handler;

    public CompleteReminderHandlerTests()
    {
        _handler = new CompleteReminderHandler(_reminderRepoMock, _userContextMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenReminderExistsAndBelongsToUser_CompletesTaskAndReturnsSuccess()
    {
        // Arrange
        const int intervalDays = 5;
        Reminder reminder = new ReminderBuilder()
            .WithIntervalDays(intervalDays)
            .Build();

        _reminderRepoMock.GetByIdAsync(reminder.Id, Arg.Any<CancellationToken>())
            .Returns(reminder);

        _userContextMock.UserId.Returns(reminder.UserId);

        var command = new CompleteReminderCommand { ReminderId = reminder.Id };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reminder.LastDoneAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        reminder.NextDueAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(intervalDays), TimeSpan.FromSeconds(2));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenReminderDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var nonExistentReminderId = Guid.NewGuid();
        _reminderRepoMock.GetByIdAsync(nonExistentReminderId, Arg.Any<CancellationToken>())
            .Returns((Reminder?)null);

        var command = new CompleteReminderCommand { ReminderId = nonExistentReminderId };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reminder.NotFound");
        result.Error.Message.Should().Be($"Reminder {nonExistentReminderId} not found.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenReminderBelongsToDifferentUser_ReturnsConflictFailure()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder().Build();

        _reminderRepoMock.GetByIdAsync(reminder.Id, Arg.Any<CancellationToken>())
            .Returns(reminder);

        var otherUserId = Guid.NewGuid();
        _userContextMock.UserId.Returns(otherUserId);

        var command = new CompleteReminderCommand { ReminderId = reminder.Id };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Access.Denied");
        result.Error.Message.Should().Be("You are not the owner of this reminder.");
    }
}
