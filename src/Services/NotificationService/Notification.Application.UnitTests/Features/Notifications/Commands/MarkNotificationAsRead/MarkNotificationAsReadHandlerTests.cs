using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadHandlerTests
{
    private readonly INotificationRepository _notificationRepoMock = Substitute.For<INotificationRepository>();
    private readonly MarkNotificationAsReadHandler _handler;

    public MarkNotificationAsReadHandlerTests()
    {
        _handler = new MarkNotificationAsReadHandler(_notificationRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNotificationExistsAndBelongsToUser_MarksAsReadAndReturnsSuccess()
    {
        // Arrange
        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithIsRead(false)
            .Build();

        _notificationRepoMock.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notification.Id,
            UserId = notification.UserId
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNotificationDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var nonExistentNotificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _notificationRepoMock.GetByIdAsync(nonExistentNotificationId, Arg.Any<CancellationToken>())
            .Returns((Domain.Entities.Notification?)null);

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = nonExistentNotificationId,
            UserId = userId
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.NotFound");
        result.Error.Message.Should().Be($"Notification {nonExistentNotificationId} not found.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNotificationBelongsToDifferentUser_ReturnsNotFoundFailure()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(otherUserId)
            .WithIsRead(false)
            .Build();

        _notificationRepoMock.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);

        var requestUserId = Guid.NewGuid();
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notification.Id,
            UserId = requestUserId
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.NotFound");
        result.Error.Message.Should().Be($"Notification {notification.Id} not found.");
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNotificationAlreadyRead_ReturnsSuccessAndDoesNotMarkAgain()
    {
        // Arrange
        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithIsRead(true)
            .Build();

        _notificationRepoMock.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notification.Id,
            UserId = notification.UserId
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
    }
}
