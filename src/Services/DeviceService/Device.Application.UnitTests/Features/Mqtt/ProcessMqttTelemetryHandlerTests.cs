using BuildingBlocks.Domain.Results;
using BuildingBlocks.IntegrationEvents.Events.Telemetrys;
using Device.Application.Features.Mqtt;
using MassTransit;

namespace Device.Application.UnitTests.Features.Mqtt;

public class ProcessMqttTelemetryHandlerTests
{
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly ISensorRepository _sensorRepoMock = Substitute.For<ISensorRepository>();
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();
    private readonly ILogger<ProcessMqttTelemetryHandler> _loggerMock = Substitute.For<ILogger<ProcessMqttTelemetryHandler>>();
    private readonly ProcessMqttTelemetryHandler _handler;

    public ProcessMqttTelemetryHandlerTests()
    {
        _handler = new ProcessMqttTelemetryHandler(
            _controllerRepoMock,
            _sensorRepoMock,
            _publishEndpointMock,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidBatch_PublishesTelemetryBatchReceivedFromDeviceEventWithControllerAndUserId()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:FF")
            .Build();
        Sensor sensor = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .Build();

        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:FF", Arg.Any<CancellationToken>())
            .Returns(controller);
        _sensorRepoMock.GetAllSensorsAsync(controller.Id, Arg.Any<CancellationToken>())
            .Returns([sensor]);

        var recordedAt = DateTime.UtcNow;
        var command = new ProcessMqttTelemetryCommand
        {
            TopicMac = "AABBCCDDEEFF",
            PayloadMacAddress = "AA:BB:CC:DD:EE:FF",
            Items =
            [
                new MqttTelemetryItemDto
                {
                    SensorId = sensor.Id,
                    Value = 25.4,
                    ExternalMessageId = "AABBCCDDEEFF-0001-00000001-0",
                    RecordedAt = recordedAt
                }
            ]
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<TelemetryBatchReceivedFromDeviceEvent>(e =>
                e.ControllerId == controller.Id &&
                e.UserId == controller.UserId &&
                e.MacAddress == "AA:BB:CC:DD:EE:FF" &&
                e.Items.Count == 1 &&
                e.Items[0].SensorId == sensor.Id &&
                Math.Abs(e.Items[0].Value - 25.4) < 0.0001 &&
                e.Items[0].ExternalMessageId == "AABBCCDDEEFF-0001-00000001-0"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMixedOwnedAndForeignSensors_PublishesOnlyOwnedSensorsAndSkipsForeign()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:11")
            .Build();
        Sensor ownedSensor = new SensorBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .Build();
        Guid foreignSensorId = Guid.NewGuid();

        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:11", Arg.Any<CancellationToken>())
            .Returns(controller);
        _sensorRepoMock.GetAllSensorsAsync(controller.Id, Arg.Any<CancellationToken>())
            .Returns([ownedSensor]);

        var command = new ProcessMqttTelemetryCommand
        {
            TopicMac = "AABBCCDDEE11",
            Items =
            [
                new MqttTelemetryItemDto
                {
                    SensorId = ownedSensor.Id,
                    Value = 24.1,
                    ExternalMessageId = "MSG-OWNED-1",
                    RecordedAt = DateTime.UtcNow
                },
                new MqttTelemetryItemDto
                {
                    SensorId = foreignSensorId,
                    Value = 99.9,
                    ExternalMessageId = "MSG-FOREIGN-1",
                    RecordedAt = DateTime.UtcNow
                }
            ]
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<TelemetryBatchReceivedFromDeviceEvent>(e =>
                e.Items.Count == 1 &&
                e.Items[0].SensorId == ownedSensor.Id &&
                e.Items[0].ExternalMessageId == "MSG-OWNED-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyItemsOrOverLimitOrUnknownController_DoesNotPublishAndReturnsCleanly()
    {
        // Arrange
        _controllerRepoMock.GetByMacAddressAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Controller?)null);

        var emptyCommand = new ProcessMqttTelemetryCommand
        {
            TopicMac = "AABBCCDDEE22",
            Items = []
        };

        var overLimitItems = Enumerable.Range(0, 51)
            .Select(i => new MqttTelemetryItemDto
            {
                SensorId = Guid.NewGuid(),
                Value = i,
                ExternalMessageId = $"MSG-{i}",
                RecordedAt = DateTime.UtcNow
            })
            .ToList();

        var overLimitCommand = new ProcessMqttTelemetryCommand
        {
            TopicMac = "AABBCCDDEE22",
            Items = overLimitItems
        };

        var unknownControllerCommand = new ProcessMqttTelemetryCommand
        {
            TopicMac = "AABBCCDDEE22",
            Items =
            [
                new MqttTelemetryItemDto
                {
                    SensorId = Guid.NewGuid(),
                    Value = 22.0,
                    ExternalMessageId = "MSG-UNKNOWN",
                    RecordedAt = DateTime.UtcNow
                }
            ]
        };

        // Act
        Result r1 = await _handler.Handle(emptyCommand, CancellationToken.None);
        Result r2 = await _handler.Handle(overLimitCommand, CancellationToken.None);
        Result r3 = await _handler.Handle(unknownControllerCommand, CancellationToken.None);

        // Assert
        r1.IsSuccess.Should().BeTrue();
        r2.IsSuccess.Should().BeTrue();
        r3.IsSuccess.Should().BeTrue();
        await _publishEndpointMock.DidNotReceiveWithAnyArgs()
            .Publish(Arg.Any<TelemetryBatchReceivedFromDeviceEvent>(), Arg.Any<CancellationToken>());
    }
}
