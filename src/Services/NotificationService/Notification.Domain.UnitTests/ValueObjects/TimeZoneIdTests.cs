using Contracts.Results;
using FluentAssertions;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.UnitTests.ValueObjects;

public class TimeZoneIdTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceTimeZone_ReturnsFailure(string? invalidTz)
    {
        // Act
        Result<TimeZoneId> result = TimeZoneId.Create(invalidTz!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeZoneId.Invalid");
        result.Error.Message.Should().Be("Time zone is required.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("Invalid/Timezone")]
    [InlineData("Europe/Atlantis")]
    public void Create_WithUnrecognizedTimeZone_ReturnsFailure(string unrecognizedTz)
    {
        // Act
        Result<TimeZoneId> result = TimeZoneId.Create(unrecognizedTz);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeZoneId.Invalid");
        result.Error.Message.Should().Contain("is not recognized.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("UTC", "UTC")]
    [InlineData("  UTC  ", "UTC")]
    public void Create_WithValidTimeZone_ReturnsSuccessAndTrimsValue(string input, string expected)
    {
        // Act
        Result<TimeZoneId> result = TimeZoneId.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        TimeZoneId tz1 = TimeZoneId.Create("UTC").Value;
        TimeZoneId tz2 = TimeZoneId.Create("  UTC  ").Value;

        // Assert
        tz1.Should().Be(tz2);
    }
}
