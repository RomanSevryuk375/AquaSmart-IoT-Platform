using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.Reminders.Commands.CreateReminder;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.Reminders.Commands.CreateReminder;

public class CreateReminderHandlerTests
{
    private readonly IReminderRepository _reminderRepoMock = Substitute.For<IReminderRepository>();
    private readonly CreateReminderHandler _handler;

    public CreateReminderHandlerTests()
    {
        _handler = new CreateReminderHandler(_reminderRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidCommand_CreatesReminderAndReturnsSuccess()
    {
        // Arrange
        var command = new CreateReminderCommand
        {
            UserId = Guid.NewGuid(),
            EcosystemId = Guid.NewGuid(),
            TaskName = "Water the plants",
            IntervalDays = 3
        };

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _reminderRepoMock.Received(1).AddAsync(
            Arg.Is<Reminder>(r =>
                r.UserId == command.UserId &&
                r.EcosystemId == command.EcosystemId &&
                r.TaskName.Value == command.TaskName &&
                r.IntervalDays == command.IntervalDays),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithInvalidCommand_ReturnsFailureResult()
    {
        // Arrange
        var command = new CreateReminderCommand
        {
            UserId = Guid.NewGuid(),
            EcosystemId = Guid.NewGuid(),
            TaskName = "Invalid",
            IntervalDays = -1
        };

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _reminderRepoMock.DidNotReceive().AddAsync(Arg.Any<Reminder>(), Arg.Any<CancellationToken>());
    }
}
