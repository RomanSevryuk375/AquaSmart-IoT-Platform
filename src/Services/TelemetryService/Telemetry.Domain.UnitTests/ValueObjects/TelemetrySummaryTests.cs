// Ignore Spelling: Telemetry

namespace Telemetry.Domain.UnitTests.ValueObjects;

public class TelemetrySummaryTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        double minValue = 15.0;
        double avgValue = 20.5;
        double maxValue = 25.0;
        int count = 10;

        // Act
        Result<TelemetrySummary> result = TelemetrySummary.Create(minValue, avgValue, maxValue, count);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MinValue.Should().Be(minValue);
        result.Value.AvgValue.Should().Be(avgValue);
        result.Value.MaxValue.Should().Be(maxValue);
        result.Value.Count.Should().Be(count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithInvalidCount_ReturnsValidationFailure(int invalidCount)
    {
        // Arrange & Act
        Result<TelemetrySummary> result = TelemetrySummary.Create(15.0, 20.0, 25.0, invalidCount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TelemetrySummary.Invalid");
        result.Error.Message.Should().Be("Data points count must be greater than zero.");
    }

    [Fact]
    public void Create_WithMinValueGreaterThanMaxValue_ReturnsValidationFailure()
    {
        // Arrange & Act
        Result<TelemetrySummary> result = TelemetrySummary.Create(30.0, 20.0, 10.0, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TelemetrySummary.Invalid");
        result.Error.Message.Should().Be(
            "MinValue cannot be greater than MaxValue.; AvgValue must be between MinValue and MaxValue.");
    }

    [Fact]
    public void Create_WithAvgValueLessThanMinValue_ReturnsValidationFailure()
    {
        // Arrange & Act
        Result<TelemetrySummary> result = TelemetrySummary.Create(15.0, 10.0, 25.0, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TelemetrySummary.Invalid");
        result.Error.Message.Should().Be("AvgValue must be between MinValue and MaxValue.");
    }

    [Fact]
    public void Create_WithAvgValueGreaterThanMaxValue_ReturnsValidationFailure()
    {
        // Arrange & Act
        Result<TelemetrySummary> result = TelemetrySummary.Create(15.0, 30.0, 25.0, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TelemetrySummary.Invalid");
        result.Error.Message.Should().Be("AvgValue must be between MinValue and MaxValue.");
    }

    [Fact]
    public void Create_WithMultipleValidationFailures_ReturnsValidationFailureWithConcatenatedMessages()
    {
        // Arrange & Act
        Result<TelemetrySummary> result = TelemetrySummary.Create(30.0, 5.0, 10.0, 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TelemetrySummary.Invalid");
        result.Error.Message.Should().Contain("Data points count must be greater than zero.");
        result.Error.Message.Should().Contain("MinValue cannot be greater than MaxValue.");
        result.Error.Message.Should().Contain("AvgValue must be between MinValue and MaxValue.");
    }
}
