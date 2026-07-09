using AutoMapper;
using Contracts.Events.TelemetryEvents;
using MassTransit;
using Telemetry.Application.DTOs;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.UnitTests.Features.Telemetry;

public class AddTelemetryBatchHandlerTests
{
    private readonly ITelemetryRawDataRepository _telemetryRepositoryMock;
    private readonly ISensorRepository _sensorRepositoryMock;
    private readonly IEcosystemRepository _ecosystemRepositoryMock;
    private readonly IPublishEndpoint _publishEndpointMock;
    private readonly ITelemetryNotifier _realtimeNotifierMock;
    private readonly IMapper _mapperMock;
    private readonly AddTelemetryBatchHandler _handler;

    public AddTelemetryBatchHandlerTests()
    {
        _telemetryRepositoryMock = Substitute.For<ITelemetryRawDataRepository>();
        _sensorRepositoryMock = Substitute.For<ISensorRepository>();
        _ecosystemRepositoryMock = Substitute.For<IEcosystemRepository>();
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _realtimeNotifierMock = Substitute.For<ITelemetryNotifier>();
        _mapperMock = Substitute.For<IMapper>();

        _mapperMock.Map<TelemetryReceivedEvent>(Arg.Any<TelemetryBatchEventItem>())
            .Returns(callInfo =>
            {
                TelemetryBatchEventItem item = callInfo.Arg<TelemetryBatchEventItem>();
                return new TelemetryReceivedEvent
                {
                    SensorId = item.SensorId,
                    Value = item.Value,
                    RecordedAt = item.RecordedAt
                };
            });

        _mapperMock.Map<TelemetryRawChartPointDto>(Arg.Any<TelemetryBatchEventItem>())
            .Returns(callInfo =>
            {
                TelemetryBatchEventItem item = callInfo.Arg<TelemetryBatchEventItem>();
                return new TelemetryRawChartPointDto
                {
                    SensorId = item.SensorId,
                    Value = item.Value,
                    RecordedAt = item.RecordedAt
                };
            });

        _handler = new AddTelemetryBatchHandler(
            _telemetryRepositoryMock,
            _sensorRepositoryMock,
            _ecosystemRepositoryMock,
            _publishEndpointMock,
            _mapperMock,
            _realtimeNotifierMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenEcosystemNotFound_ReturnsFailure()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns((Ecosystem?)null);

        var command = new AddTelemetryBatchCommand
        {
            ControllerId = controllerId,
            Items = new List<TelemetryBatchEventItem>()
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ecosystem.NotFound");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoSensorsFoundForEcosystem_ReturnsFailure()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).Build();
        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(ecosystem);

        _sensorRepositoryMock.GetAllByEcosystemId(ecosystem.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Sensor>());

        var command = new AddTelemetryBatchCommand
        {
            ControllerId = controllerId,
            Items = new List<TelemetryBatchEventItem>()
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sensor.NotFound");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenValidBatchSent_SavesDataUpdatesSensorPublishesEventAndNotifiesSignalR()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();

        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(ecosystem);

        _sensorRepositoryMock.GetAllByEcosystemId(ecosystem.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Sensor> { sensor });

        DateTime recordedAt = DateTime.UtcNow;
        var batchItem = new TelemetryBatchEventItem
        {
            SensorId = sensor.Id,
            Value = 42.5,
            ExternalMessageId = "ext_msg_987",
            RecordedAt = recordedAt
        };

        var command = new AddTelemetryBatchCommand
        {
            ControllerId = controllerId,
            Items = new List<TelemetryBatchEventItem> { batchItem }
        };

        _telemetryRepositoryMock.GetByExternalMessageIdAsync(batchItem.ExternalMessageId, Arg.Any<CancellationToken>())
            .Returns((RawTelemetry?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.LastValue.Should().BeApproximately(42.5, 0.001);

        await _telemetryRepositoryMock.Received(1).AddAsync(
            Arg.Is<RawTelemetry>(t => t.SensorId == sensor.Id &&
            Math.Abs(t.Value - 42.5) < 0.001 && t.ExternalMessageId == "ext_msg_987" && t.RecordedAt == recordedAt),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<TelemetryReceivedEvent>(e => e.SensorId == sensor.Id &&
            Math.Abs(e.Value - 42.5) < 0.001 && e.RecordedAt == recordedAt),
            Arg.Any<CancellationToken>());

        await _realtimeNotifierMock.Received(1).TelemetryRawReceived(
            ecosystem.Id.ToString(),
            Arg.Is<TelemetryRawChartPointDto>(p => p.SensorId == sensor.Id &&
            Math.Abs(p.Value - 42.5) < 0.001 && p.RecordedAt == recordedAt));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSensorNotFoundInEcosystem_SkipsItem()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();

        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(ecosystem);

        _sensorRepositoryMock.GetAllByEcosystemId(ecosystem.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Sensor> { sensor });

        var differentSensorId = Guid.NewGuid();
        var batchItem = new TelemetryBatchEventItem
        {
            SensorId = differentSensorId,
            Value = 42.5,
            ExternalMessageId = "ext_msg_987",
            RecordedAt = DateTime.UtcNow
        };

        var command = new AddTelemetryBatchCommand
        {
            ControllerId = controllerId,
            Items = new List<TelemetryBatchEventItem> { batchItem }
        };

        _telemetryRepositoryMock.GetByExternalMessageIdAsync(
            batchItem.ExternalMessageId, Arg.Any<CancellationToken>())
            .Returns((RawTelemetry?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _telemetryRepositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<RawTelemetry>(), Arg.Any<CancellationToken>());
        await _publishEndpointMock.DidNotReceive()
            .Publish(Arg.Any<TelemetryReceivedEvent>(), Arg.Any<CancellationToken>());
        await _realtimeNotifierMock.DidNotReceive()
            .TelemetryRawReceived(Arg.Any<string>(), Arg.Any<TelemetryRawChartPointDto>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenExternalMessageIdAlreadyExists_SkipsItemForIdempotency()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();

        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(ecosystem);

        _sensorRepositoryMock.GetAllByEcosystemId(ecosystem.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Sensor> { sensor });

        var batchItem = new TelemetryBatchEventItem
        {
            SensorId = sensor.Id,
            Value = 42.5,
            ExternalMessageId = "ext_msg_exists",
            RecordedAt = DateTime.UtcNow
        };

        var command = new AddTelemetryBatchCommand
        {
            ControllerId = controllerId,
            Items = new List<TelemetryBatchEventItem> { batchItem }
        };

        RawTelemetry existingTelemetry = new RawTelemetryBuilder()
            .WithSensorId(sensor.Id)
            .WithExternalMessageId(batchItem.ExternalMessageId)
            .Build();

        _telemetryRepositoryMock.GetByExternalMessageIdAsync(
            batchItem.ExternalMessageId, Arg.Any<CancellationToken>())
            .Returns(existingTelemetry);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _telemetryRepositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<RawTelemetry>(), Arg.Any<CancellationToken>());
        await _publishEndpointMock.DidNotReceive()
            .Publish(Arg.Any<TelemetryReceivedEvent>(), Arg.Any<CancellationToken>());
        await _realtimeNotifierMock.DidNotReceive()
            .TelemetryRawReceived(Arg.Any<string>(), Arg.Any<TelemetryRawChartPointDto>());
    }
}
