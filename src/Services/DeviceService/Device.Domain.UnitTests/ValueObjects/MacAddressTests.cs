using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace Device.Domain.UnitTests.ValueObjects;

public class MacAddressTests
{
    [Fact]
    public void Create_WithValidMacAddress_ReturnsSuccessAndUpperCasedValue()
    {
        // Arrange
        string validMac = $" {TestConstants.ValidMacAddress.ToLowerInvariant()} ";

        // Act
        Result<MacAddress> result = MacAddress.Create(validMac);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(TestConstants.ValidMacAddress);
    }

    [Theory]
    [InlineData("aa:bb:cc:dd:ee:ff", "AA:BB:CC:DD:EE:FF")]
    [InlineData("aa-bb-cc-dd-ee-ff", "AA:BB:CC:DD:EE:FF")]
    [InlineData("AA-BB-CC-DD-EE-FF", "AA:BB:CC:DD:EE:FF")]
    [InlineData("aabbccddeeff", "AA:BB:CC:DD:EE:FF")]
    [InlineData("AABBCCDDEEFF", "AA:BB:CC:DD:EE:FF")]
    [InlineData("  aaBbCcDdEeFf  ", "AA:BB:CC:DD:EE:FF")]
    public void Create_WithCompactHexOrColonFormat_ProducesCanonicalUppercaseColonFormat(
        string input,
        string expectedCanonical)
    {
        // Act
        Result<MacAddress> result = MacAddress.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expectedCanonical);
        result.Value.ToCompactHex().Should().Be("AABBCCDDEEFF");
    }

    [Theory]
    [InlineData("AA:BB:CC:DD:EE:FF", "AABBCCDDEEFF")]
    [InlineData("aa:bb:cc:dd:ee:ff", "AABBCCDDEEFF")]
    [InlineData("aa-bb-cc-dd-ee-ff", "AABBCCDDEEFF")]
    [InlineData("aabbccddeeff", "AABBCCDDEEFF")]
    [InlineData("  00-1A-2b-3c-4D-5e ", "001A2B3C4D5E")]
    public void NormalizeToCompactHex_WithColonsDashesOrLowercase_Returns12CharUppercaseHex(
        string input,
        string expectedCompactHex)
    {
        // Act
        Result<string> result = MacAddress.NormalizeToCompactHex(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedCompactHex);
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
        string invalidMac = TestConstants.ValidMacAddress[..8];

        // Act
        Result<MacAddress> result = MacAddress.Create(invalidMac);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ControllerErrors.InvalidMacAddressLength);
    }

    [Theory]
    [InlineData("00:1A:2B:3C:4D:5Z")]
    [InlineData("00-1A-2B-3C-4D_5E")]
    [InlineData("AABBCCDDEEFG")]
    [InlineData("AABBCCDDEEF")]
    [InlineData("AABBCCDDEEFF1")]
    public void Create_WithInvalidLengthOrChars_ReturnsFailure(string invalidMac)
    {
        // Act
        Result<MacAddress> result = MacAddress.Create(invalidMac);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
