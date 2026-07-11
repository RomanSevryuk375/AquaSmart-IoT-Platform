// Ignore Spelling: Telemetry

namespace Telemetry.Domain.UnitTests.Entities;

public class AggregateTelemetryTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        DateTime periodStart = DateTime.UtcNow.Date;
        PeriodType period = PeriodType.Hourly;
        double minValue = 10.0;
        double maxValue = 30.0;
        double avgValue = 20.0;
        int dataPointsCount = 5;

        // Act
        Result<AggregateTelemetry> result = AggregateTelemetry.Create(
            id, sensorId, ecosystemId, periodStart, period, minValue, maxValue, avgValue, dataPointsCount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.SensorId.Should().Be(sensorId);
        result.Value.PeriodStart.Should().Be(periodStart);
        result.Value.Period.Should().Be(period);
        result.Value.Summary.Should().NotBeNull();
        result.Value.Summary.MinValue.Should().Be(minValue);
        result.Value.Summary.MaxValue.Should().Be(maxValue);
        result.Value.Summary.AvgValue.Should().Be(avgValue);
        result.Value.Summary.Count.Should().Be(dataPointsCount);
        result.Value.IsAggregated.Should().BeFalse();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithInvalidSummaryData_ReturnsFailureFromSummary()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        DateTime periodStart = DateTime.UtcNow.Date;
        PeriodType period = PeriodType.Hourly;
        double minValue = 30.0;
        double maxValue = 10.0;
        double avgValue = 20.0;
        int dataPointsCount = 5;

        // Act
        Result<AggregateTelemetry> result = AggregateTelemetry.Create(
            id, sensorId, ecosystemId, periodStart, period, minValue, maxValue, avgValue, dataPointsCount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TelemetrySummary.Invalid");
        result.Error.Message.Should().Contain("MinValue cannot be greater than MaxValue.");
    }

    [Fact]
    public void MarkAsAggregated_WhenCalled_SetsIsAggregatedToTrueAndIncrementsVersion()
    {
        // Arrange
        AggregateTelemetry aggregateTelemetry = new AggregateTelemetryBuilder().Build();
        Guid initialVersion = aggregateTelemetry.Version;

        // Act
        aggregateTelemetry.MarkAsAggregated();

        // Assert
        aggregateTelemetry.IsAggregated.Should().BeTrue();
        aggregateTelemetry.Version.Should().NotBe(initialVersion);
    }
}
