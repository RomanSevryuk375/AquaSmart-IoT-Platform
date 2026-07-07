using Contracts.Results;
using FluentAssertions;
using IdentityService.Domain.Entities;

namespace Identity.Domain.UnitTests.Entities;

public class SubscriptionTests
{
    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        // Act
        Result<Subscription> result = Subscription.Create(
            Guid.NewGuid(),
            rawName: "",
            rawPrice: 10.0m,
            durationDays: 30,
            permissions: []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        result.Error.Message.Should().Be("Name cannot be empty.");
    }

    [Fact]
    public void Create_WithNegativePrice_ReturnsFailure()
    {
        // Act
        Result<Subscription> result = Subscription.Create(
            Guid.NewGuid(),
            rawName: "Premium",
            rawPrice: -5.0m,
            durationDays: 30,
            permissions: []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.Invalid");
        result.Error.Message.Should().Be("Amount cannot be negative.");
    }

    [Fact]
    public void Create_WithNegativeDuration_ReturnsFailure()
    {
        // Act
        Result<Subscription> result = Subscription.Create(
            Guid.NewGuid(),
            rawName: "Premium",
            rawPrice: 10.0m,
            durationDays: -1,
            permissions: []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Subscription.Invalid");
        result.Error.Message.Should().Be("Duration cannot be negative.");
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndInitializesFields()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        string name = "Basic Plan";
        decimal price = 9.99m;
        int duration = 30;
        var permissions = new List<string> { "read:telemetry" };
        var tolerance = TimeSpan.FromSeconds(5);

        // Act
        Result<Subscription> result = Subscription.Create(
            subscriptionId,
            name,
            price,
            duration,
            permissions);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Subscription sub = result.Value;
        sub.Id.Should().Be(subscriptionId);
        sub.Name.Value.Should().Be(name);
        sub.Price.Amount.Should().Be(price);
        sub.DurationDays.Should().Be(duration);
        sub.Permissions.Should().BeEquivalentTo(permissions);
        sub.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        sub.Version.Should().BeEmpty();
    }
}
