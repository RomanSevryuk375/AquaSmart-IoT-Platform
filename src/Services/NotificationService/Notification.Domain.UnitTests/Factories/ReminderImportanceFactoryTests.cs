using Contracts.Enums;
using FluentAssertions;
using Notification.Domain.Factories;

namespace Notification.Domain.UnitTests.Factories;

public class ReminderImportanceFactoryTests
{
    [Theory]
    // Critical: <= -24 hours
    [InlineData(-25, NotificationLevel.Critical)]
    [InlineData(-24, NotificationLevel.Critical)]
    // Warning: <= 0 and > -24 hours
    [InlineData(-23.9, NotificationLevel.Warning)]
    [InlineData(-12, NotificationLevel.Warning)]
    [InlineData(-1, NotificationLevel.Warning)]
    [InlineData(0, NotificationLevel.Warning)]
    // Info: > 0 hours
    [InlineData(0.1, NotificationLevel.Info)]
    [InlineData(1, NotificationLevel.Info)]
    [InlineData(23.9, NotificationLevel.Info)]
    [InlineData(24, NotificationLevel.Info)]
    [InlineData(48, NotificationLevel.Info)]
    public void Evaluate_BasedOnTimeRemaining_ReturnsExpectedImportance(double hoursOffset,
        NotificationLevel expectedLevel)
    {
        // Arrange
        DateTime nextDueAt = DateTime.UtcNow.AddHours(hoursOffset);

        // Act
        NotificationLevel result = ReminderImportanceFactory.Evaluate(nextDueAt);

        // Assert
        result.Should().Be(expectedLevel);
    }
}
