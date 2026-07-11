// Ignore Spelling: Telemetry

namespace Telemetry.Domain.UnitTests.Entities;

public class RawTelemetryTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        double value = 25.5;
        string externalMessageId = "msg-12345";
        DateTime recordedAt = DateTime.UtcNow;

        // Act
        Result<RawTelemetry> result = RawTelemetry.Create(
            id, sensorId, ecosystemId, value, externalMessageId, recordedAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.SensorId.Should().Be(sensorId);
        result.Value.Value.Should().Be(value);
        result.Value.ExternalMessageId.Should().Be(externalMessageId);
        result.Value.RecordedAt.Should().Be(recordedAt);
        result.Value.IsAggregated.Should().BeFalse();
    }

    [Fact]
    public void Create_WithRecordedAtInFuture_ReturnsValidationFailure()
    {
        // Arrange
        DateTime futureDate = DateTime.UtcNow.AddMinutes(10);

        // Act
        Result<RawTelemetry> result = RawTelemetry.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 25.5, "msg-123", futureDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RawTelemetry.Invalid");
        result.Error.Message.Should().Be("recordedAt cannot be in the future.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Create_WithEmptyExternalMessageId_ReturnsValidationFailure(string invalidMessageId)
    {
        // Arrange & Act
        Result<RawTelemetry> result = RawTelemetry.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 25.5, invalidMessageId, DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RawTelemetry.Invalid");
        result.Error.Message.Should().Be("externalMessageId must not be empty.");
    }

    [Fact]
    public void Create_WithMultipleErrors_ReturnsValidationFailureWithConcatenatedMessages()
    {
        // Arrange
        DateTime futureDate = DateTime.UtcNow.AddMinutes(10);

        // Act
        Result<RawTelemetry> result = RawTelemetry.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 25.5, "", futureDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RawTelemetry.Invalid");
        result.Error.Message.Should().Contain("recordedAt cannot be in the future.");
        result.Error.Message.Should().Contain("externalMessageId must not be empty.");
    }

    [Fact]
    public void MarkAsAggregated_WhenCalled_SetsIsAggregatedToTrueAndIncrementsVersion()
    {
        // Arrange
        RawTelemetry rawTelemetry = new RawTelemetryBuilder().Build();
        Guid initialVersion = rawTelemetry.Version;

        // Act
        rawTelemetry.MarkAsAggregated();

        // Assert
        rawTelemetry.IsAggregated.Should().BeTrue();
        rawTelemetry.Version.Should().NotBe(initialVersion);
    }
}
