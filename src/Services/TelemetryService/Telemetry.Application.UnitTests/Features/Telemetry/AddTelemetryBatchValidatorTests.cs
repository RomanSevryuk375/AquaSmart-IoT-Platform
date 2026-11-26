using BuildingBlocks.IntegrationEvents.Events.Telemetrys;
using FluentValidation.Results;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;

namespace Telemetry.Application.UnitTests.Features.Telemetry;

public class AddTelemetryBatchValidatorTests
{
    private readonly AddTelemetryBatchValidator _validator = new();

    private static TelemetryBatchEventItem CreateValidItem(double value = 25.0, string? extId = null) =>
        new()
        {
            SensorId = Guid.NewGuid(),
            Value = value,
            ExternalMessageId = extId ?? "AABBCCDDEEFF-0001-00000001-0",
            RecordedAt = DateTime.UtcNow
        };

    [Fact]
    public void Validate_WithEmptyItems_FailsValidation()
    {
        // Arrange
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "AA:BB:CC:DD:EE:FF",
            DeviceToken = "ak_token",
            Items = []
        };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddTelemetryBatchCommand.Items));
    }

    [Fact]
    public void Validate_WithNullItems_FailsValidationWithoutThrowing()
    {
        // Arrange
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "AA:BB:CC:DD:EE:FF",
            DeviceToken = "ak_token",
            Items = null!
        };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddTelemetryBatchCommand.Items));
    }

    [Fact]
    public void Validate_WithOver50Items_FailsValidation()
    {
        // Arrange
        List<TelemetryBatchEventItem> items = Enumerable.Range(0, 51)
            .Select(i => CreateValidItem(extId: $"ext-{i}"))
            .ToList();

        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "AA:BB:CC:DD:EE:FF",
            DeviceToken = "ak_token",
            Items = items
        };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddTelemetryBatchCommand.Items));
    }

    [Fact]
    public void Validate_WithEmptySensorId_FailsValidation()
    {
        // Arrange
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "AA:BB:CC:DD:EE:FF",
            DeviceToken = "ak_token",
            Items =
            [
                new TelemetryBatchEventItem
                {
                    SensorId = Guid.Empty,
                    Value = 22.5,
                    ExternalMessageId = "ext-1",
                    RecordedAt = DateTime.UtcNow
                }
            ]
        };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyExternalMessageId_FailsValidation(string extId)
    {
        // Arrange
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "AA:BB:CC:DD:EE:FF",
            DeviceToken = "ak_token",
            Items = [CreateValidItem(extId: extId)]
        };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithExternalMessageIdOver100Chars_FailsValidation()
    {
        // Arrange
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "AA:BB:CC:DD:EE:FF",
            DeviceToken = "ak_token",
            Items = [CreateValidItem(extId: new string('x', 101))]
        };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-mac")]
    [InlineData("AA:BB:CC:DD:EE:ZZ")]
    public void Validate_WithInvalidMacAddress_FailsValidation(string macAddress)
    {
        // Arrange
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = macAddress,
            DeviceToken = "ak_token",
            Items = [CreateValidItem()]
        };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddTelemetryBatchCommand.MacAddress));
    }

    [Theory]
    [InlineData("AA:BB:CC:DD:EE:FF", 0.0)]
    [InlineData("AABBCCDDEEFF", -2.5)]
    public void Validate_WithZeroOrNegativeValue_PassesValidation(string macAddress, double value)
    {
        // Arrange
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = macAddress,
            DeviceToken = "ak_token",
            Items = [CreateValidItem(value: value)]
        };

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
