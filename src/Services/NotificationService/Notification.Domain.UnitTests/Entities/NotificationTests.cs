using Contracts.Enums;
using Contracts.Results;
using FluentAssertions;
using Notification.TestShared.Builders;
using DomainNotification = Notification.Domain.Entities.Notification;

namespace Notification.Domain.UnitTests.Entities;

public class NotificationTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndInitializesProperties()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        NotificationLevel level = NotificationLevel.Warning;
        string message = "Water temperature is slightly elevated.";

        // Act
        Result<DomainNotification> result = DomainNotification.Create(
            notificationId, userId, ecosystemId, level, message);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(notificationId);
        result.Value.UserId.Should().Be(userId);
        result.Value.EcosystemId.Should().Be(ecosystemId);
        result.Value.Level.Should().Be(level);
        result.Value.Message.Value.Should().Be(message);
        result.Value.IsRead.Should().BeFalse();
        result.Value.IsPublished.Should().BeFalse();
        result.Value.PublishedAt.Should().BeNull();
        result.Value.RetryCount.Should().Be(0);
        result.Value.FailureReason.Should().BeNull();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.Version.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_WithInvalidMessage_ReturnsFailure()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string invalidMessage = "";

        // Act
        Result<DomainNotification> result = DomainNotification.Create(
            notificationId, userId, null, NotificationLevel.Info, invalidMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MessageText.Invalid");
    }

    [Fact]
    public void MarkAsPublished_WhenNotPublished_PublishesAndIncrementsVersion()
    {
        // Arrange
        DomainNotification notification = new NotificationBuilder().WithIsPublished(false).Build();
        Guid initialVersion = notification.Version;

        // Act
        notification.MarkAsPublished();

        // Assert
        notification.IsPublished.Should().BeTrue();
        notification.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        notification.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void MarkAsPublished_WhenAlreadyPublished_DoesNotPublishOrChangeVersion()
    {
        // Arrange
        DomainNotification notification = new NotificationBuilder().WithIsPublished(true).Build();
        Guid initialVersion = notification.Version;
        DateTime? initialPublishedAt = notification.PublishedAt;

        // Act
        notification.MarkAsPublished();

        // Assert
        notification.IsPublished.Should().BeTrue();
        notification.PublishedAt.Should().Be(initialPublishedAt);
        notification.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void MarkAsRead_WhenNotRead_MarksAsReadAndIncrementsVersion()
    {
        // Arrange
        DomainNotification notification = new NotificationBuilder().WithIsRead(false).Build();
        Guid initialVersion = notification.Version;

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_DoesNotChangeVersion()
    {
        // Arrange
        DomainNotification notification = new NotificationBuilder().WithIsRead(true).Build();
        Guid initialVersion = notification.Version;

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void MarkAsFailure_IncrementsRetryCountSetsFailureAndIncrementsVersion()
    {
        // Arrange
        DomainNotification notification = new NotificationBuilder().WithIsPublished(true).Build();
        Guid initialVersion = notification.Version;
        string exceptionMessage = "SMTP Connection Timeout";

        // Act
        notification.MarkAsFailure(exceptionMessage);

        // Assert
        notification.IsPublished.Should().BeFalse();
        notification.RetryCount.Should().Be(1);
        notification.FailureReason.Should().Be(exceptionMessage);
        notification.Version.Should().NotBe(initialVersion);
    }
}
