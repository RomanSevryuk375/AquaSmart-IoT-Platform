using Contracts.Results;
using FluentAssertions;
using Notification.Domain.Entities;
using Notification.TestShared.Builders;

namespace Notification.Domain.UnitTests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndInitializesProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string email = "user@example.com";
        string timeZone = "UTC";
        string phoneNumber = "+375291112233";
        bool emailEnable = true;
        bool tgEnable = true;
        long? telegramChatId = 123456789L;
        bool enable = true;
        DateTime createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        Result<User> result = User.Create(
            userId,
            email,
            timeZone,
            phoneNumber,
            emailEnable,
            tgEnable,
            telegramChatId,
            enable,
            createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.Email.Value.Should().Be(email);
        result.Value.TimeZone.Value.Should().Be(timeZone);
        result.Value.PhoneNumber.Value.Should().Be(phoneNumber);
        result.Value.EmailEnable.Should().Be(emailEnable);
        result.Value.TgEnable.Should().Be(tgEnable);
        result.Value.TelegramChatId.Should().Be(telegramChatId);
        result.Value.IsNotifyEnabled.Should().Be(enable);
        result.Value.CreatedAt.Should().Be(createdAt);
        result.Value.Version.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_WithInvalidPhoneNumber_ReturnsFailure()
    {
        // Act
        Result<User> result = User.Create(
            Guid.NewGuid(),
            "user@example.com",
            "UTC",
            "invalid-phone",
            true,
            false,
            null,
            true,
            DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.Invalid");
    }

    [Fact]
    public void Create_WithInvalidTimeZone_ReturnsFailure()
    {
        // Act
        Result<User> result = User.Create(
            Guid.NewGuid(),
            "user@example.com",
            "Invalid-Tz",
            "+375291112233",
            true,
            false,
            null,
            true,
            DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeZoneId.Invalid");
    }

    [Fact]
    public void Create_WithInvalidEmail_ReturnsFailure()
    {
        // Act
        Result<User> result = User.Create(
            Guid.NewGuid(),
            "invalid-email",
            "UTC",
            "+375291112233",
            true,
            false,
            null,
            true,
            DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
    }

    [Fact]
    public void Create_WithTelegramEnabledAndChatIdNull_ReturnsFailure()
    {
        // Act
        Result<User> result = User.Create(
            Guid.NewGuid(),
            "user@example.com",
            "UTC",
            "+375291112233",
            true,
            true,
            null,
            true,
            DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Invalid");
        result.Error.Message.Should().Be("Telegram chat id must not be empty if tgEnable is true.");
    }

    [Fact]
    public void UpdateContacts_WithValidData_UpdatesEmailAndPhoneAndIncrementsVersion()
    {
        // Arrange
        User user = new UserBuilder().Build();
        Guid initialVersion = user.Version;
        string newEmail = "updated@example.com";
        string newPhone = "80447654321";

        // Act
        Result result = user.UpdateContacts(newEmail, newPhone);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be(newEmail);
        user.PhoneNumber.Value.Should().Be(newPhone);
        user.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void UpdateContacts_WithInvalidEmail_ReturnsFailureAndDoesNotChangeVersion()
    {
        // Arrange
        User user = new UserBuilder().Build();
        Guid initialVersion = user.Version;

        // Act
        Result result = user.UpdateContacts("invalid-email", "+375291112233");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
        user.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void UpdateContacts_WithInvalidPhone_ReturnsFailureAndDoesNotChangeVersion()
    {
        // Arrange
        User user = new UserBuilder().Build();
        Guid initialVersion = user.Version;

        // Act
        Result result = user.UpdateContacts("user@example.com", "invalid-phone");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.Invalid");
        user.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void SetNotificationPreferences_WithValidData_UpdatesPreferencesAndIncrementsVersion()
    {
        // Arrange
        User user = new UserBuilder().WithTgEnable(false).Build();
        Guid initialVersion = user.Version;

        // Act
        Result result = user.SetNotificationPreferences(false, true, 987654321L);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.EmailEnable.Should().BeFalse();
        user.TgEnable.Should().BeTrue();
        user.TelegramChatId.Should().Be(987654321L);
        user.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetNotificationPreferences_WithTelegramEnabledAndChatIdNull_ReturnsFailureAndDoesNotChangeVersion()
    {
        // Arrange
        User user = new UserBuilder().WithTgEnable(false).Build();
        Guid initialVersion = user.Version;

        // Act
        Result result = user.SetNotificationPreferences(true, true, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Invalid");
        result.Error.Message.Should().Be("Telegram Chat ID is required if Telegram notifications are enabled.");
        user.Version.Should().Be(initialVersion);
    }
}
