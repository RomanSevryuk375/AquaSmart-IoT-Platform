using Contracts.Results;
using FluentAssertions;
using Identity.TestShared.Builders;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Events;
using IdentityService.Domain.ValueObjects;

namespace Identity.Domain.UnitTests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithInvalidName_ReturnsFailure()
    {
        // Act
        Result<User> result = User.Create(
            Guid.NewGuid(),
            rawName: "",
            rawEmail: "test@example.com",
            rawPhoneNumber: "+375291112233",
            subscriptionId: Guid.NewGuid(),
            rawTimeZone: "UTC");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
    }

    [Fact]
    public void Create_WithInvalidEmail_ReturnsFailure()
    {
        // Act
        Result<User> result = User.Create(
            Guid.NewGuid(),
            rawName: "Test User",
            rawEmail: "invalid-email",
            rawPhoneNumber: "+375291112233",
            subscriptionId: Guid.NewGuid(),
            rawTimeZone: "UTC");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
    }

    [Fact]
    public void Create_WithInvalidPhoneNumber_ReturnsFailure()
    {
        // Act
        Result<User> result = User.Create(
            Guid.NewGuid(),
            rawName: "Test User",
            rawEmail: "test@example.com",
            rawPhoneNumber: "12345",
            subscriptionId: Guid.NewGuid(),
            rawTimeZone: "UTC");

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
            rawName: "Test User",
            rawEmail: "test@example.com",
            rawPhoneNumber: "+375291112233",
            subscriptionId: Guid.NewGuid(),
            rawTimeZone: "Invalid-TimeZone");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeZoneId.Invalid");
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccessRaisesCreatedEventAndInitializesFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string name = "John Doe";
        string email = "john.doe@example.com";
        string phone = "+375447654321";
        var subscriptionId = Guid.NewGuid();
        string timeZone = "UTC";
        var tolerance = TimeSpan.FromSeconds(5);

        // Act
        Result<User> result = User.Create(
            userId,
            name,
            email,
            phone,
            subscriptionId,
            timeZone);

        // Assert
        result.IsSuccess.Should().BeTrue();
        User user = result.Value;

        user.Id.Should().Be(userId);
        user.Name.Value.Should().Be(name);
        user.Email.Should().Be(email);
        user.UserName.Should().Be(email);
        user.PhoneNumber.Should().Be(phone);
        user.SubscriptionId.Should().Be(subscriptionId);
        user.TimeZone.Value.Should().Be(timeZone);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        user.SubscriptionEndDate.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        user.ConcurrencyStamp.Should().NotBeNullOrWhiteSpace();

        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedDomainEvent>();

        UserCreatedDomainEvent createdEvent = user.DomainEvents.OfType<UserCreatedDomainEvent>().Single();
        createdEvent.UserId.Should().Be(user.Id);
        createdEvent.Email.Should().Be(user.Email);
        createdEvent.PhoneNumber.Should().Be(user.PhoneNumber);
        createdEvent.CreatedAt.Should().Be(user.CreatedAt);
        createdEvent.TimeZone.Should().Be(user.TimeZone.Value);
    }

    [Fact]
    public void UpdateProfile_WithInvalidName_ReturnsFailureDoesNotMutateStateOrRaiseEvent()
    {
        // Arrange
        User user = new UserBuilder().Build();
        Name initialName = user.Name;
        string? initialPhone = user.PhoneNumber;
        string? initialStamp = user.ConcurrencyStamp;

        // Act
        Result result = user.UpdateProfile("", "+375291112233");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        user.Name.Should().Be(initialName);
        user.PhoneNumber.Should().Be(initialPhone);
        user.ConcurrencyStamp.Should().Be(initialStamp);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateProfile_WithInvalidPhoneNumber_ReturnsFailureDoesNotMutateStateOrRaiseEvent()
    {
        // Arrange
        User user = new UserBuilder().Build();
        Name initialName = user.Name;
        string? initialPhone = user.PhoneNumber;
        string? initialStamp = user.ConcurrencyStamp;

        // Act
        Result result = user.UpdateProfile("New Name", "invalid-phone");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.Invalid");
        user.Name.Should().Be(initialName);
        user.PhoneNumber.Should().Be(initialPhone);
        user.ConcurrencyStamp.Should().Be(initialStamp);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateProfile_WithValidData_UpdatesFieldsChangesConcurrencyStampAndRaisesUpdatedEvent()
    {
        // Arrange
        User user = new UserBuilder().WithName("Old Name").WithPhoneNumber("+375291112233").Build();
        string? initialStamp = user.ConcurrencyStamp;

        // Act
        Result result = user.UpdateProfile("New Name", "+375336667788");

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Name.Value.Should().Be("New Name");
        user.PhoneNumber.Should().Be("+375336667788");
        user.ConcurrencyStamp.Should().NotBe(initialStamp);

        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserUpdatedDomainEvent>();

        UserUpdatedDomainEvent updatedEvent = user.DomainEvents.OfType<UserUpdatedDomainEvent>().Single();
        updatedEvent.UserId.Should().Be(user.Id);
        updatedEvent.Email.Should().Be(user.Email);
        updatedEvent.Name.Should().Be("New Name");
        updatedEvent.PhoneNumber.Should().Be("+375336667788");
    }

    [Fact]
    public void SetSubscription_WhenCalled_UpdatesSubscriptionFieldsAndChangesConcurrencyStamp()
    {
        // Arrange
        User user = new UserBuilder().Build();
        string? initialStamp = user.ConcurrencyStamp;
        var newSubId = Guid.NewGuid();
        int durationDays = 90;
        var tolerance = TimeSpan.FromSeconds(5);

        // Act
        user.SetSubscription(newSubId, durationDays);

        // Assert
        user.SubscriptionId.Should().Be(newSubId);
        user.SubscriptionEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(durationDays), tolerance);
        user.ConcurrencyStamp.Should().NotBe(initialStamp);
        user.DomainEvents.Should().BeEmpty();
    }
}
