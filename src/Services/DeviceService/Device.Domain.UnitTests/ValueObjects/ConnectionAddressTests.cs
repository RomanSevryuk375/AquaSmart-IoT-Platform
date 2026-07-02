namespace Device.Domain.UnitTests.ValueObjects;

public class ConnectionAddressTests
{
    [Theory]
    [InlineData(ConnectionProtocol.I2C)]
    [InlineData(ConnectionProtocol.OneWire)]
    [InlineData(ConnectionProtocol.Analog)]
    [InlineData(ConnectionProtocol.Digital)]
    public void Create_WithEmptyAddress_ReturnsValidationFailure(ConnectionProtocol protocol)
    {
        // Act
        Result<ConnectionAddress> result = ConnectionAddress.Create(protocol, "   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(CommonErrors.AddressEmpty);
    }

    [Fact]
    public void Create_WithUnsupportedProtocol_ReturnsValidationFailure()
    {
        // Arrange
        var unsupportedProtocol = (ConnectionProtocol)999;

        // Act
        Result<ConnectionAddress> result = ConnectionAddress.Create(
            unsupportedProtocol, TestConstants.ValidI2cAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(CommonErrors.InvalidAddressProtocol);
    }

    [Theory]
    [InlineData("0x00")]
    [InlineData("0x38")]
    [InlineData("0x7F")]
    public void Create_WithValidI2cAddress_ReturnsSuccess(string validAddress)
    {
        // Act
        Result<ConnectionAddress> result = ConnectionAddress.Create(ConnectionProtocol.I2C, validAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Address.Should().Be(validAddress);
        result.Value.Protocol.Should().Be(ConnectionProtocol.I2C);
    }

    [Theory]
    [InlineData("38")]
    [InlineData("0xGG")]
    public void Create_WithInvalidI2cFormat_ReturnsValidationFailure(string invalidAddress)
    {
        // Act
        Result<ConnectionAddress> result = ConnectionAddress.Create(ConnectionProtocol.I2C, invalidAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(CommonErrors.I2cInvalidFormat);
    }

    [Theory]
    [InlineData("0x80")]
    [InlineData("0xFF")]
    public void Create_WithI2cAddressOutOfRange_ReturnsValidationFailure(string invalidAddress)
    {
        // Act
        Result<ConnectionAddress> result = ConnectionAddress.Create(ConnectionProtocol.I2C, invalidAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(CommonErrors.I2cInvalid7bitAddress);
    }

    [Fact]
    public void Create_WithValidOneWireAddress_ReturnsSuccess()
    {
        // Arrange 
        string validAddress = "28FF4A1B2C3D4E5F";

        // Act
        Result<ConnectionAddress> result = ConnectionAddress.Create(ConnectionProtocol.OneWire, validAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Address.Should().Be(validAddress);
    }

    [Theory]
    [InlineData("PA5")]
    [InlineData("GPIO13")]
    [InlineData("D2")]
    [InlineData("13")]
    public void Create_WithValidDigitalAddress_ReturnsSuccess(string validAddress)
    {
        // Act
        Result<ConnectionAddress> result = ConnectionAddress.Create(ConnectionProtocol.Digital, validAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Address.Should().Be(validAddress);
    }

    [Theory]
    [InlineData("A0")]
    [InlineData("ADC1_CH3")]
    [InlineData("3")]
    public void Create_WithValidAnalogAddress_ReturnsSuccess(string validAddress)
    {
        // Act
        Result<ConnectionAddress> result = ConnectionAddress.Create(ConnectionProtocol.Analog, validAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Address.Should().Be(validAddress);
    }
}
