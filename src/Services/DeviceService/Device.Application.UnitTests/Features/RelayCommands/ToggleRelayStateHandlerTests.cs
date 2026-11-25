using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Application.Features.Mqtt;
using Device.Application.Features.RelayCommands.Command.ToggleRelayMode;
using Device.Application.Features.RelayCommands.Command.ToggleRelayState;
using Device.Application.Handlers;
using Device.Domain.Events.ControllerEvents;
using Device.Domain.Events.RelayCommandEvents;
using Device.Domain.Events.RelayEvents;
using Microsoft.Extensions.Options;

namespace Device.Application.UnitTests.Features.RelayCommands;

public class ToggleRelayStateHandlerTests
{
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IRelayCommandsRepository _queueRepoMock = Substitute.For<IRelayCommandsRepository>();
    private readonly IMqttCommandPublisher _mqttPublisherMock = Substitute.For<IMqttCommandPublisher>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task ToggleRelayState_WithValidRelay_CreatesPendingCommandDoesNotMutateIsActiveAndReturnsQueuedDto()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { CommandTtlMinutes = 10 });
        var handler = new ToggleRelayStateHandler(_relayRepoMock, _queueRepoMock, options);
        Relay relay = new RelayBuilder().AsActive(false).Build();
        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        // Act
        Result<RelayCommandQueuedResponse> result = await handler.Handle(
            new ToggleRelayStateCommand { RelayId = relay.Id, UserId = relay.UserId },
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CommandId.Should().NotBeEmpty();
        result.Value.RelayId.Should().Be(relay.Id);
        result.Value.TargetState.Should().BeTrue();
        result.Value.Status.Should().Be(CommandStatus.Pending.ToString());
        relay.IsActive.Should().BeFalse();
        relay.DomainEvents.Should().BeEmpty();
        await _queueRepoMock.Received(1).AddAsync(
            Arg.Is<RelayCommand>(c =>
                c.ControllerId == relay.ControllerId &&
                c.RelayId == relay.Id &&
                c.TargetState &&
                c.Status == CommandStatus.Pending),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ToggleRelayMode_WithValidRelay_UpdatesIsManualIncrementsConfigVersionAndPublishesModeSyncedAndConfigNotify()
    {
        // Arrange
        var handler = new ToggleRelayModeHandler(_relayRepoMock, _controllerRepoMock);
        Controller controller = new ControllerBuilder().Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).AsAuto().Build();
        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        // Act
        Result<bool> result = await handler.Handle(
            new ToggleRelayModeCommand { RelayId = relay.Id, UserId = relay.UserId },
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        relay.IsManual.Should().BeTrue();
        relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayModeChangedDomainEvent>()
            .Which.Should().Match<RelayModeChangedDomainEvent>(e =>
                e.ControllerId == controller.Id &&
                e.RelayId == relay.Id &&
                e.IsManual);
        controller.ConfigVersion.Should().Be(2);
        controller.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ControllerConfigChangedDomainEvent>()
            .Which.ConfigVersion.Should().Be(2);
    }

    [Fact]
    public async Task ProcessCommandAck_WithCompletedStatus_UpdatesRelayIsActiveMarksCompletedAndPublishesRelayStateSyncedEvent()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProcessCommandAckHandler>>();
        var handler = new ProcessCommandAckHandler(_controllerRepoMock, _queueRepoMock, _relayRepoMock, logger);

        Controller controller = new ControllerBuilder().WithMacAddress("AA:BB:CC:DD:EE:01").Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).AsActive(false).Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithTargetState(true)
            .Build();

        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:01", Arg.Any<CancellationToken>()).Returns(controller);
        _queueRepoMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(command);
        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        // Act
        Result result = await handler.Handle(new ProcessCommandAckCommand
        {
            TopicMac = "AABBCCDDEE01",
            CommandId = command.Id,
            Status = "completed",
            ExecutedAt = DateTime.UtcNow
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        command.Status.Should().Be(CommandStatus.Completed);
        relay.IsActive.Should().BeTrue();
        relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayStateChangedDomainEvent>()
            .Which.TargetState.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessCommandAck_WithFailedStatus_MarksFailedWithoutChangingRelayIsActive()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProcessCommandAckHandler>>();
        var handler = new ProcessCommandAckHandler(_controllerRepoMock, _queueRepoMock, _relayRepoMock, logger);

        Controller controller = new ControllerBuilder().WithMacAddress("AA:BB:CC:DD:EE:02").Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).AsActive(false).Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithTargetState(true)
            .Build();

        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:02", Arg.Any<CancellationToken>()).Returns(controller);
        _queueRepoMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(command);

        // Act
        Result result = await handler.Handle(new ProcessCommandAckCommand
        {
            TopicMac = "AABBCCDDEE02",
            CommandId = command.Id,
            Status = "failed",
            ErrorMessage = "GPIO fault"
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        command.Status.Should().Be(CommandStatus.Failed);
        command.ErrorMessage.Should().Be("GPIO fault");
        relay.IsActive.Should().BeFalse();
        await _relayRepoMock.DidNotReceiveWithAnyArgs().GetByIdAsync(Guid.Empty, CancellationToken.None);
    }

    [Fact]
    public async Task ProcessCommandAck_WhenCommandAlreadyTerminal_IsIdempotentNoOp()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProcessCommandAckHandler>>();
        var handler = new ProcessCommandAckHandler(_controllerRepoMock, _queueRepoMock, _relayRepoMock, logger);

        Controller controller = new ControllerBuilder().WithMacAddress("AA:BB:CC:DD:EE:03").Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithTargetState(true)
            .Build();
        command.Cancel();
        Guid versionBefore = command.Version;

        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:03", Arg.Any<CancellationToken>()).Returns(controller);
        _queueRepoMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(command);

        // Act
        Result result = await handler.Handle(new ProcessCommandAckCommand
        {
            TopicMac = "AABBCCDDEE03",
            CommandId = command.Id,
            Status = "completed"
        }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        command.Status.Should().Be(CommandStatus.Cancelled);
        command.Version.Should().Be(versionBefore);
        await _relayRepoMock.DidNotReceiveWithAnyArgs().GetByIdAsync(Guid.Empty, CancellationToken.None);
    }

    [Fact]
    public async Task RelayCommandCreatedHandler_WhenMqttConnected_PublishesAndMarksCommandSent()
    {
        // Arrange
        var logger = Substitute.For<ILogger<RelayCommandCreatedHandler>>();
        var handler = new RelayCommandCreatedHandler(
            _controllerRepoMock, _queueRepoMock, _mqttPublisherMock, _unitOfWorkMock, logger);

        Controller controller = new ControllerBuilder().WithMacAddress("AA:BB:CC:DD:EE:04").Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithTargetState(true)
            .Build();

        _queueRepoMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(command);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);
        _mqttPublisherMock.PublishRelayCommandAsync(
            controller.MacAddress.Value,
            Arg.Any<RelayCommandMqttPayload>(),
            Arg.Any<CancellationToken>()).Returns(true);

        // Act
        await handler.Handle(new RelayCommandCreatedDomainEvent
        {
            CommandId = command.Id,
            ControllerId = controller.Id,
            RelayId = command.RelayId,
            TargetState = command.TargetState,
            ExpireAt = command.ExpireAt,
            CreatedAt = command.CreatedAt
        }, CancellationToken.None);

        // Assert
        command.Status.Should().Be(CommandStatus.Sent);
        command.AttemptCount.Should().Be(1);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RelayCommandCreatedHandler_WhenControllerOffline_KeepsCommandPendingWithoutPublishing()
    {
        // Arrange
        var logger = Substitute.For<ILogger<RelayCommandCreatedHandler>>();
        var handler = new RelayCommandCreatedHandler(
            _controllerRepoMock, _queueRepoMock, _mqttPublisherMock, _unitOfWorkMock, logger);

        Controller offlineController = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:09")
            .AsOffline()
            .Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(offlineController.Id)
            .WithTargetState(true)
            .Build();

        _queueRepoMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(command);
        _controllerRepoMock.GetByIdAsync(offlineController.Id, Arg.Any<CancellationToken>()).Returns(offlineController);

        // Act
        await handler.Handle(new RelayCommandCreatedDomainEvent
        {
            CommandId = command.Id,
            ControllerId = offlineController.Id,
            RelayId = command.RelayId,
            TargetState = command.TargetState,
            ExpireAt = command.ExpireAt,
            CreatedAt = command.CreatedAt
        }, CancellationToken.None);

        // Assert
        command.Status.Should().Be(CommandStatus.Pending);
        command.AttemptCount.Should().Be(0);
        await _mqttPublisherMock.DidNotReceiveWithAnyArgs()
            .PublishRelayCommandAsync(default!, default!, default);
    }

    [Fact]
    public async Task ControllerConfigChangedHandler_PublishesConfigNotifyToMqtt()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ControllerConfigChangedHandler>>();
        var handler = new ControllerConfigChangedHandler(_mqttPublisherMock, logger);
        _mqttPublisherMock.PublishConfigNotifyAsync(
            "AA:BB:CC:DD:EE:05",
            Arg.Any<ConfigNotifyMqttPayload>(),
            Arg.Any<CancellationToken>()).Returns(true);

        // Act
        await handler.Handle(new ControllerConfigChangedDomainEvent
        {
            ControllerId = Guid.NewGuid(),
            MacAddress = "AA:BB:CC:DD:EE:05",
            ConfigVersion = 4,
            Reason = "sensor_added"
        }, CancellationToken.None);

        // Assert
        await _mqttPublisherMock.Received(1).PublishConfigNotifyAsync(
            "AA:BB:CC:DD:EE:05",
            Arg.Is<ConfigNotifyMqttPayload>(p => p.Version == 4 && p.Reason == "sensor_added"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessDeviceStatus_OnlineAndOfflineLwt_UpdatesControllerAndIsIdempotent()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProcessDeviceStatusHandler>>();
        var handler = new ProcessDeviceStatusHandler(_controllerRepoMock, logger);
        Controller controller = new ControllerBuilder().WithMacAddress("AA:BB:CC:DD:EE:06").AsOffline().Build();
        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:06", Arg.Any<CancellationToken>()).Returns(controller);

        // Act 1: Stale retained offline when already offline -> no-op
        Guid versionInitial = controller.Version;
        await handler.Handle(new ProcessDeviceStatusCommand
        {
            TopicMac = "AABBCCDDEE06",
            Status = "offline"
        }, CancellationToken.None);
        controller.Version.Should().Be(versionInitial);

        // Act 2: Online heartbeat
        await handler.Handle(new ProcessDeviceStatusCommand
        {
            TopicMac = "AABBCCDDEE06",
            Status = "online",
            FirmwareVersion = "1.0.5",
            IpAddress = "192.168.1.50",
            Rssi = -61,
            UptimeSec = 3600
        }, CancellationToken.None);

        // Assert online
        controller.IsOnline.Should().BeTrue();
        controller.FirmwareVersion.Should().Be("1.0.5");
        controller.IpAddress.Should().Be("192.168.1.50");
        controller.Rssi.Should().Be(-61);
        controller.UptimeSec.Should().Be(3600);

        // Act 3: LWT offline
        await handler.Handle(new ProcessDeviceStatusCommand
        {
            TopicMac = "AABBCCDDEE06",
            Status = "offline"
        }, CancellationToken.None);

        // Assert offline without alert event
        controller.IsOnline.Should().BeFalse();
        controller.DomainEvents.Should().BeEmpty();
    }
}
