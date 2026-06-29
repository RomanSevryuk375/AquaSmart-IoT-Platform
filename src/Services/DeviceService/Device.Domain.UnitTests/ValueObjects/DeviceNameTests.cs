using Contracts.Constants;
using Contracts.Results;
using Device.Domain.ValueObjects;
using Device.TestShared.Constants;
using FluentAssertions;

namespace Device.Domain.UnitTests.ValueObjects;

public class DeviceNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSuccessAndTrimmedValue()
    {
        // Arrange
        string validName = $"  {TestConstants.ValidDeviceName}  ";

        // Act
        Result<DeviceName> result = DeviceName.Create(validName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(TestConstants.ValidDeviceName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Create_WithEmptyValue_ReturnsValidationFailure(string invalidName)
    {
        // Act
        Result<DeviceName> result = DeviceName.Create(invalidName);

        // Assert
        result.IsFailure.Should().BeTrue();
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
        result.Error.Message.Should().Be(CommonErrors.InvalidNameLength);
    }
}
