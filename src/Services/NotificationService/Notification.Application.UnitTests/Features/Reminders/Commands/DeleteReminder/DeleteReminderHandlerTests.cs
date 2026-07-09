using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.Reminders.Commands.DeleteReminder;
using Notification.Domain.Interfaces;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.Reminders.Commands.DeleteReminder;

public class DeleteReminderHandlerTests
{
    private readonly IReminderRepository _reminderRepoMock = Substitute.For<IReminderRepository>();
    private readonly DeleteReminderHandler _handler;

    public DeleteReminderHandlerTests()
    {
        _handler = new DeleteReminderHandler(_reminderRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenCalled_DeletesReminderAndReturnsSuccess()
    {
        // Arrange
        var reminderId = Guid.NewGuid();
        var command = new DeleteReminderCommand { ReminderId = reminderId };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _reminderRepoMock.Received(1).DeleteAsync(reminderId, Arg.Any<CancellationToken>());
    }
}
