using Contracts.Enums;
using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.Alerts.Commands.SendSubscriptionAlert;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.Alerts.Commands.SendSubscriptionAlert;

public class SendSubscriptionAlertHandlerTests
{
    private readonly IUserRepository _userRepoMock = Substitute.For<IUserRepository>();
    private readonly INotificationRepository _notificationRepoMock = Substitute.For<INotificationRepository>();
    private readonly SendSubscriptionAlertHandler _handler;

    public SendSubscriptionAlertHandlerTests()
    {
        _handler = new SendSubscriptionAlertHandler(_userRepoMock, _notificationRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserExists_CreatesNotificationAndReturnsSuccess()
    {
        // Arrange
        User user = new UserBuilder().Build();
        _userRepoMock.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var command = new SendSubscriptionAlertCommand
        {
            UserId = user.Id,
            NewSubscriptionId = Guid.NewGuid(),
            OccurredOn = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _notificationRepoMock.Received(1).AddAsync(
            Arg.Is<Domain.Entities.Notification>(n =>
                n.UserId == user.Id &&
                n.Level == NotificationLevel.Warning &&
                n.EcosystemId == null &&
                n.Message.Value == "Your subscription has expired and was downgraded to Free. Premium features and telegram alerts are now disabled."),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new SendSubscriptionAlertCommand
        {
            UserId = userId,
            NewSubscriptionId = Guid.NewGuid(),
            OccurredOn = DateTime.UtcNow
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
        result.Error.Message.Should().Be($"User {userId} not found.");

        await _notificationRepoMock.DidNotReceive().AddAsync(Arg.Any<Domain.Entities.Notification>(), Arg.Any<CancellationToken>());
    }
}
