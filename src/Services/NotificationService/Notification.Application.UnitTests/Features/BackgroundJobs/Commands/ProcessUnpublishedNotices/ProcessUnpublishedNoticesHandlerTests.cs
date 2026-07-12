using Contracts.Constants;
using Contracts.Results;
using FluentAssertions;
using MassTransit;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;
using Notification.Application.InternalEvents;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;

public class ProcessUnpublishedNoticesHandlerTests
{
    private readonly INotificationRepository _notificationRepoMock = Substitute.For<INotificationRepository>();
    private readonly IUserRepository _userRepoMock = Substitute.For<IUserRepository>();
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();
    private readonly ProcessUnpublishedNoticesHandler _handler;

    public ProcessUnpublishedNoticesHandlerTests()
    {
        _handler = new ProcessUnpublishedNoticesHandler(
            _notificationRepoMock,
            _userRepoMock,
            _publishEndpointMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoUnpublishedNotifications_ReturnsSuccess()
    {
        // Arrange
        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification>());

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userRepoMock.DidNotReceive().GetAllUsersByIdAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserHasBothChannelsEnabled_PublishesBothAndMarksAsPublished()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEnable(true)
            .WithTgEnable(true, 123456789)
            .WithEmailEnable(true)
            .WithEmail("user@test.com")
            .Build();

        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeTrue();
        notification.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        notification.FailureReason.Should().BeNull();
        notification.RetryCount.Should().Be(0);

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendTelegramCommand>(cmd =>
                cmd.NotificationId == notification.Id &&
                cmd.ChatId == 123456789 &&
                cmd.Message == notification.Message.Value),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendEmailCommand>(cmd =>
                cmd.NotificationId == notification.Id &&
                cmd.Email == "user@test.com" &&
                cmd.Message == notification.Message.Value),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserHasOnlyTgEnabled_PublishesTgOnlyAndMarksAsPublished()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEnable(true)
            .WithTgEnable(true, 123456789)
            .WithEmailEnable(false)
            .Build();

        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeTrue();

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendTelegramCommand>(cmd =>
                cmd.NotificationId == notification.Id &&
                cmd.ChatId == 123456789 &&
                cmd.Message == notification.Message.Value),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserHasOnlyEmailEnabled_PublishesEmailOnlyAndMarksAsPublished()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEnable(true)
            .WithTgEnable(false)
            .WithEmailEnable(true)
            .WithEmail("user@test.com")
            .Build();

        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeTrue();

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendEmailCommand>(cmd =>
                cmd.NotificationId == notification.Id &&
                cmd.Email == "user@test.com" &&
                cmd.Message == notification.Message.Value),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<SendTelegramCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFound_MarksNotificationAsFailure()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(nonExistentUserId)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeFalse();
        notification.RetryCount.Should().Be(1);
        notification.FailureReason.Should().Be(ErrorMessages.NotificationProvider.UserDisabledOrNotFound);

        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserIsNotifyEnabledIsFalse_MarksNotificationAsFailure()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEnable(false)
            .Build();

        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeFalse();
        notification.RetryCount.Should().Be(1);
        notification.FailureReason.Should().Be(ErrorMessages.NotificationProvider.UserDisabledOrNotFound);

        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoProvidersAreEnabledForUser_MarksNotificationAsFailure()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEnable(true)
            .WithTgEnable(false)
            .WithEmailEnable(false)
            .Build();

        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeFalse();
        notification.RetryCount.Should().Be(1);
        notification.FailureReason.Should().Be(ErrorMessages.NotificationProvider.NoActiveChannels);

        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
