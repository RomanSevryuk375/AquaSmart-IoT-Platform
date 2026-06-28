using Contracts.Constants;
using Contracts.Results;
using Device.Domain.ValueObjects;
using FluentAssertions;

namespace Device.Domain.UnitTests.ValueObjects;

public class MacAddressTests
{
    [Fact]
    public void Create_WithValidMacAddress_ReturnsSuccessAndUpperCasedValue()
    {
        // Arrange
        string validMac = " 00:1a:2b:3c:4d:5e ";
        string expectedMac = "00:1A:2B:3C:4D:5E";

        // Act
        Result<MacAddress> result = MacAddress.Create(validMac);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expectedMac);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Create_WithEmptyValue_ReturnsValidationFailure(string invalidMac)
    {
        // Act
        Result<MacAddress> result = MacAddress.Create(invalidMac);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ControllerErrors.MacAddressEmpty);
    }

    [Fact]
    public void Create_WithInvalidLength_ReturnsValidationFailure()
    {
        // Arrange
        string invalidMac = "00:11:22";

        // Act
        Result<MacAddress> result = MacAddress.Create(invalidMac);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ControllerErrors.InvalidMacAddressLength);
    }

    [Theory]
    [InlineData("00:1A:2B:3C:4D:5Z")]
    [InlineData("00-1A-2B-3C-4D_5E")]
    public void Create_WithInvalidFormat_ReturnsValidationFailure(string invalidMac)
    {
        // Act
        Result<MacAddress> result = MacAddress.Create(invalidMac);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ControllerErrors.InvalidMacAddressFormat);
    }
}
