// Ignore Spelling: Telemetry Device

namespace Telemetry.Domain.UnitTests.ValueObjects;

public class DeviceNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSuccessAndTrimmedValue()
    {
        // Arrange
        string rawName = "  Room Temperature Sensor  ";
        string expectedName = "Room Temperature Sensor";

        // Act
        Result<DeviceName> result = DeviceName.Create(rawName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expectedName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Create_WithEmptyName_ReturnsValidationFailure(string invalidName)
    {
        // Arrange & Act
        Result<DeviceName> result = DeviceName.Create(invalidName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeviceName.Invalid");
        result.Error.Message.Should().Be(CommonErrors.NameEmpty);
    }

    [Fact]
    public void Create_WithTooLongName_ReturnsValidationFailure()
    {
        // Arrange
        string longName = new('A', CommonConstants.NameLength + 1);

        // Act
        Result<DeviceName> result = DeviceName.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeviceName.Invalid");
        result.Error.Message.Should().Be(CommonErrors.InvalidNameLength);
    }

    [Fact]
    public void ToString_WhenCalled_ReturnsValue()
    {
        // Arrange
        string nameValue = "PH Sensor";
        Result<DeviceName> createResult = DeviceName.Create(nameValue);
        DeviceName deviceName = createResult.Value;

        // Act
        string result = deviceName.ToString();

        // Assert
        result.Should().Be(nameValue);
    }
}
