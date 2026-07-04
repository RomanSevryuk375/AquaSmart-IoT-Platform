namespace Control.Domain.UnitTests.ValueObjects;

public class ConditionThresholdTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        double threshold = 25.5;
        double hysteresis = 0.5;

        // Act
        Result<ConditionThreshold> result = ConditionThreshold.Create(threshold, hysteresis);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Threshold.Should().Be(threshold);
        result.Value.Hysteresis.Should().Be(hysteresis);
    }

    [Fact]
    public void Create_WithNegativeHysteresis_ReturnsFailure()
    {
        // Arrange
        double threshold = 25.5;
        double hysteresis = -0.1;

        // Act
        Result<ConditionThreshold> result = ConditionThreshold.Create(threshold, hysteresis);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ConditionThreshold.Invalid");
        result.Error.Message.Should().Contain("Hysteresis cannot be negative");
    }

    [Fact]
    public void Parse_WithValidDbValue_ReturnsExpectedConditionThreshold()
    {
        // Arrange
        string dbValue = "22.5_1.2";

        // Act
        var threshold = ConditionThreshold.Parse(dbValue);

        // Assert
        threshold.Should().NotBeNull();
        threshold.Threshold.Should().Be(22.5);
        threshold.Hysteresis.Should().Be(1.2);
    }

    [Fact]
    public void ToString_ReturnsExpectedFormattedString()
    {
        // Arrange
        ConditionThreshold threshold = ConditionThreshold.Create(20.0, 0.25).Value;

        // Act
        string str = threshold.ToString();

        // Assert
        str.Should().Be("20_0.25");
    }
}
