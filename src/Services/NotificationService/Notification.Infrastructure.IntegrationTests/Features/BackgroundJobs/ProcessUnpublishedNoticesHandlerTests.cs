using Contracts.Constants;
using Contracts.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;
using Notification.Application.InternalEvents;
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
    public async Task Handle_ShouldMarkNotificationAsPublished_AndPublishCommands_WhenUserHasChannelsEnabled()
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

        await Factory.PublishEndpointMock.Received(1).Publish(
            Arg.Is<SendEmailCommand>(cmd =>
                cmd.NotificationId == notificationId &&
                cmd.Email == "user@example.com" &&
                cmd.Message == "Success notification message"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldIncrementRetryCountAndSetFailureReason_WhenUserDisabledOrNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        User user = new UserBuilder()
            .WithId(userId)
            .WithEmail("user@example.com")
            .WithEnable(false)
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
        updatedNotification.FailureReason.Should().Be(ErrorMessages.NotificationProvider.UserDisabledOrNotFound);

        await Factory.PublishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
