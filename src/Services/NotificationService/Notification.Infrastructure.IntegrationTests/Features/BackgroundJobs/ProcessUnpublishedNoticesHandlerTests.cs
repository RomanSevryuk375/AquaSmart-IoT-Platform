using Contracts.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;
using Notification.Domain.Entities;
using Notification.Infrastructure.IntegrationTests.Infrastructure;
using Notification.TestShared.Builders;
using NSubstitute;
using DomainNotification = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.IntegrationTests.Features.BackgroundJobs;

public class ProcessUnpublishedNoticesHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldMarkNotificationAsPublished_WhenProviderSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        User user = new UserBuilder()
            .WithId(userId)
            .WithEmail("user@example.com")
            .WithEnable(true)
            .WithEmailEnable(true)
            .Build();

        DomainNotification notification = new NotificationBuilder()
            .WithId(notificationId)
            .WithUserId(userId)
            .WithEcosystemId(null)
            .WithMessage("Success notification message")
            .WithIsPublished(false)
            .Build();

        DbContext.Set<User>().Add(user);
        DbContext.Set<DomainNotification>().Add(notification);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        Factory.EmailProviderMock.IsEnabled(Arg.Any<User>()).Returns(true);
        Factory.EmailProviderMock.SendAsync(
            Arg.Any<User>(),
            Arg.Is("Success notification message"),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        Factory.TgProviderMock.IsEnabled(Arg.Any<User>()).Returns(false);

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        DomainNotification? updatedNotification = await DbContext.Set<DomainNotification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        updatedNotification.Should().NotBeNull();
        updatedNotification!.IsPublished.Should().BeTrue();
        updatedNotification.FailureReason.Should().BeNull();
        updatedNotification.RetryCount.Should().Be(0);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldIncrementRetryCountAndSetFailureReason_WhenProviderFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        User user = new UserBuilder()
            .WithId(userId)
            .WithEmail("user@example.com")
            .WithEnable(true)
            .WithEmailEnable(true)
            .Build();

        DomainNotification notification = new NotificationBuilder()
            .WithId(notificationId)
            .WithUserId(userId)
            .WithEcosystemId(null)
            .WithMessage("Failure notification message")
            .WithIsPublished(false)
            .Build();

        DbContext.Set<User>().Add(user);
        DbContext.Set<DomainNotification>().Add(notification);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        Factory.EmailProviderMock.IsEnabled(Arg.Any<User>()).Returns(true);
        Factory.EmailProviderMock.SendAsync(
            Arg.Any<User>(),
            Arg.Is("Failure notification message"),
            Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Failure("Provider.Failed", "SMTP server is down")));

        Factory.TgProviderMock.IsEnabled(Arg.Any<User>()).Returns(false);

        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        DomainNotification? updatedNotification = await DbContext.Set<DomainNotification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        updatedNotification.Should().NotBeNull();
        updatedNotification!.IsPublished.Should().BeFalse();
        updatedNotification.RetryCount.Should().Be(1);
        updatedNotification.FailureReason.Should().Contain("SMTP server is down");
    }
}
