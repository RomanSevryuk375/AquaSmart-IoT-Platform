using Contracts.Events.TelemetryEvents;
using Device.Application.Features.Telemetry.Command.TransmittTelemetry;
using MassTransit;

namespace Device.Application.UnitTests.Features.Telemetry;

public class TransmitTelemetryHandlerTests
{
    private readonly ISensorRepository _sensorRepoMock = Substitute.For<ISensorRepository>();
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();
    private readonly TransmitTelemetryHandler _handler;

    public TransmitTelemetryHandlerTests()
    {
        _handler = new TransmitTelemetryHandler(_sensorRepoMock, _controllerRepoMock, _publishEndpointMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithMixedSensors_PublishesValidAndSkipsInvalid()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();
        _controllerRepoMock.GetByMacAddressAsync(TestConstants.ValidMacAddress, Arg.Any<CancellationToken>())
            .Returns(controller);

        Sensor validSensor1 = new SensorBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).Build();
        Sensor validSensor2 = new SensorBuilder().WithId(Guid.NewGuid()).WithControllerId(controller.Id).Build();

        _sensorRepoMock.GetAllSensorsAsync(controller.Id, Arg.Any<CancellationToken>())
            .Returns([validSensor1, validSensor2]);

        var invalidSensorId = Guid.NewGuid();

        var command = new TransmitTelemetryCommand
        {
            MacAddress = TestConstants.ValidMacAddress,
            DeviceToken = TestConstants.ValidTokenHash,
            Items = new List<TelemetryItem>
            {
                new() { SensorId = validSensor1.Id, Value = 25.5, RecordedAt = DateTime.UtcNow },
                new() { SensorId = invalidSensorId, Value = 100.0, RecordedAt = DateTime.UtcNow },
                new() { SensorId = validSensor2.Id, Value = 60.2, RecordedAt = DateTime.UtcNow }
            }
        };

        // Act
        Result<TelemetryTransmittedResponse> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        TelemetryTransmittedResponse response = result.Value;

        response.AcceptedCount.Should().Be(2);
        response.SkippedCount.Should().Be(1);
        response.ValidationErrors.Should().ContainSingle()
            .Which.Should().Contain(invalidSensorId.ToString());

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<TelemetryBatchEvent>(e =>
                e.ControllerId == controller.Id &&
                e.Items.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithOnlyInvalidSensors_DoesNotPublishAndSkipsAll()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();
        _controllerRepoMock.GetByMacAddressAsync(controller.MacAddress.Value, Arg.Any<CancellationToken>())
            .Returns(controller);

        _sensorRepoMock.GetAllSensorsAsync(controller.Id, Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new TransmitTelemetryCommand
        {
            MacAddress = controller.MacAddress.Value,
            Items = new List<TelemetryItem>
            {
                new() { SensorId = Guid.NewGuid(), Value = 10.0, RecordedAt = DateTime.UtcNow }
            }
        };

        // Act
        Result<TelemetryTransmittedResponse> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        TelemetryTransmittedResponse response = result.Value;

        response.AcceptedCount.Should().Be(0);
        response.SkippedCount.Should().Be(1);

        await _publishEndpointMock.DidNotReceiveWithAnyArgs().Publish(default(TelemetryBatchEvent)!, default);
    }
}
