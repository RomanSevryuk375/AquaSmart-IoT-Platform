namespace Control.Domain.UnitTests.ValueObjects;

public class VolumeTests
{
    [Fact]
    public void Create_WithValidVolume_ReturnsSuccess()
    {
        // Arrange
        double rawVolume = 120.5;

        // Act
        Result<Volume> result = Volume.Create(rawVolume);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(rawVolume);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50.0)]
    public void Create_WithZeroOrNegativeVolume_ReturnsFailure(double invalidVolume)
    {
        // Act
        Result<Volume> result = Volume.Create(invalidVolume);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Volume.Invalid");
        result.Error.Message.Should().Contain("Volume must be strictly greater than zero");
    }

    [Fact]
    public void Create_WithTooLargeVolume_ReturnsFailure()
    {
        // Arrange
        double rawVolume = 10_000_001;

        // Act
        Result<Volume> result = Volume.Create(rawVolume);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Volume.Invalid");
        result.Error.Message.Should().Contain("Volume is unrealistically large");
    }

    [Fact]
    public void ToString_ReturnsValueAsString()
    {
        // Arrange
        Volume volume = Volume.Create(250.75).Value;

        // Act
        string str = volume.ToString();

        // Assert
        str.Should().Be("250.75");
    }
}
