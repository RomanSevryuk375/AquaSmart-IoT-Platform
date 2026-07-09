using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;

public class ProcessUnpublishedNoticesHandlerTests
{
    private readonly INotificationRepository _notificationRepoMock = Substitute.For<INotificationRepository>();
    private readonly IUserRepository _userRepoMock = Substitute.For<IUserRepository>();
    private readonly INotificationProvider _emailProviderMock = Substitute.For<INotificationProvider>();
    private readonly INotificationProvider _telegramProviderMock = Substitute.For<INotificationProvider>();
    private readonly ProcessUnpublishedNoticesHandler _handler;

    public ProcessUnpublishedNoticesHandlerTests()
    {
        _handler = new ProcessUnpublishedNoticesHandler(
            _notificationRepoMock,
            _userRepoMock,
            [_emailProviderMock, _telegramProviderMock]);
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
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenOneProviderSucceedsAndOneFails_MarksNotificationAsPublished()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEnable(true)
            .Build();

        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        _emailProviderMock.IsEnabled(user).Returns(true);
        _telegramProviderMock.IsEnabled(user).Returns(true);

        _emailProviderMock.SendAsync(user, notification.Message.Value, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _telegramProviderMock.SendAsync(user, notification.Message.Value, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("Provider.Error", "Telegram API is down")));

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeTrue();
        notification.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        notification.FailureReason.Should().BeNull();
        notification.RetryCount.Should().Be(0);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenAllProvidersFail_MarksNotificationAsFailureAndIncrementsRetry()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEnable(true)
            .Build();

        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        _emailProviderMock.IsEnabled(user).Returns(true);
        _telegramProviderMock.IsEnabled(user).Returns(true);

        _emailProviderMock.SendAsync(user, notification.Message.Value, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("Email.Failed", "SMTP server error")));

        _telegramProviderMock.SendAsync(user, notification.Message.Value, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("Tg.Failed", "Telegram timeout")));

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeFalse();
        notification.PublishedAt.Should().BeNull();
        notification.RetryCount.Should().Be(1);
        notification.FailureReason.Should().Be("SMTP server error | Telegram timeout");
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
        notification.FailureReason.Should().Be("User disabled notifications or not found.");

        await _emailProviderMock.DidNotReceive().SendAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
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
        notification.FailureReason.Should().Be("User disabled notifications or not found.");

        await _emailProviderMock.DidNotReceive().SendAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoProvidersAreEnabledForUser_MarksNotificationAsFailure()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEnable(true)
            .Build();

        Domain.Entities.Notification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithIsPublished(false)
            .Build();

        _notificationRepoMock.GetUnpublishedNotificationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.Notification> { notification });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        _emailProviderMock.IsEnabled(user).Returns(false);
        _telegramProviderMock.IsEnabled(user).Returns(false);

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsPublished.Should().BeFalse();
        notification.RetryCount.Should().Be(1);
        notification.FailureReason.Should().Be("No enabled notification providers found for user.");

        await _emailProviderMock.DidNotReceive().SendAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _telegramProviderMock.DidNotReceive().SendAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
