using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.Mqtt;
using Device.Domain.Events.RelayEvents;

namespace Device.Application.UnitTests.Features.Mqtt;

public class ProcessRelayStateSyncHandlerTests
{
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly IRelayCommandsRepository _commandsRepoMock = Substitute.For<IRelayCommandsRepository>();
    private readonly ILogger<ProcessRelayStateSyncHandler> _loggerMock = Substitute.For<ILogger<ProcessRelayStateSyncHandler>>();
    private readonly ProcessRelayStateSyncHandler _handler;

    public ProcessRelayStateSyncHandlerTests()
    {
        _handler = new ProcessRelayStateSyncHandler(
            _controllerRepoMock,
            _relayRepoMock,
            _commandsRepoMock,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_WithSourceManual_UpdatesStateAndModeCancelsAllPendingAndSentCommandsAndPublishesSyncedEvents()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:51")
            .Build();
        Relay relay = new RelayBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .AsActive(true)
            .AsAuto()
            .Build();

        RelayCommand pendingCmd = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithTargetState(true)
            .Build();
        RelayCommand sentCmd = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithTargetState(true)
            .Build();
        sentCmd.MarkAsSent();

        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:51", Arg.Any<CancellationToken>())
            .Returns(controller);
        _relayRepoMock.GetAllByControllerId(controller.Id, Arg.Any<CancellationToken>())
            .Returns([relay]);
        _commandsRepoMock.GetActiveByRelayIdAsync(relay.Id, Arg.Any<CancellationToken>())
            .Returns([pendingCmd, sentCmd]);

        var command = new ProcessRelayStateSyncCommand
        {
            TopicMac = "AABBCCDDEE51",
            PayloadMacAddress = "AA:BB:CC:DD:EE:51",
            Source = "manual",
            RecordedAt = DateTime.UtcNow,
            Relays =
            [
                new MqttRelayStateItemDto
                {
                    RelayId = relay.Id,
                    State = false,
                    IsManual = true
                }
            ]
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.IsActive.Should().BeFalse();
        relay.IsManual.Should().BeTrue();
        pendingCmd.Status.Should().Be(CommandStatus.Cancelled);
        sentCmd.Status.Should().Be(CommandStatus.Cancelled);
        relay.DomainEvents.Should().ContainSingle(e => e is RelayStateChangedDomainEvent);
        relay.DomainEvents.Should().ContainSingle(e => e is RelayModeChangedDomainEvent);
    }

    [Fact]
    public async Task Handle_WithSourceBoot_UpdatesPhysicalIsActivePreservesExistingIsManualAndPublishesStateSyncedEvent()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:52")
            .Build();
        Relay relay = new RelayBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .AsActive(true)
            .AsAuto()
            .Build();

        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:52", Arg.Any<CancellationToken>())
            .Returns(controller);
        _relayRepoMock.GetAllByControllerId(controller.Id, Arg.Any<CancellationToken>())
            .Returns([relay]);

        var command = new ProcessRelayStateSyncCommand
        {
            TopicMac = "AABBCCDDEE52",
            Source = "boot",
            RecordedAt = DateTime.UtcNow,
            Relays =
            [
                new MqttRelayStateItemDto
                {
                    RelayId = relay.Id,
                    State = false,
                    IsManual = true // Must be ignored on boot so AUTO automation is preserved!
                }
            ]
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.IsActive.Should().BeFalse();
        relay.IsManual.Should().BeFalse();
        relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayStateChangedDomainEvent>()
            .Which.TargetState.Should().BeFalse();
        await _commandsRepoMock.DidNotReceiveWithAnyArgs()
            .GetActiveByRelayIdAsync(Guid.Empty, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WithSourceSync_UpdatesPhysicalIsActivePreservesExistingIsManualAndDoesNotCancelManualMode()
    {
        // Arrange
        Controller controller = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:53")
            .Build();
        Relay manualRelay = new RelayBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .AsActive(false)
            .Build(); // IsManual = true by default

        _controllerRepoMock.GetByMacAddressAsync("AA:BB:CC:DD:EE:53", Arg.Any<CancellationToken>())
            .Returns(controller);
        _relayRepoMock.GetAllByControllerId(controller.Id, Arg.Any<CancellationToken>())
            .Returns([manualRelay]);

        var command = new ProcessRelayStateSyncCommand
        {
            TopicMac = "AABBCCDDEE53",
            Source = "sync",
            RecordedAt = DateTime.UtcNow,
            Relays =
            [
                new MqttRelayStateItemDto
                {
                    RelayId = manualRelay.Id,
                    State = true,
                    IsManual = false // Must NOT overwrite existing server manual mode
                }
            ]
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        manualRelay.IsActive.Should().BeTrue();
        manualRelay.IsManual.Should().BeTrue();
        manualRelay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayStateChangedDomainEvent>()
            .Which.TargetState.Should().BeTrue();
        await _commandsRepoMock.DidNotReceiveWithAnyArgs()
            .GetActiveByRelayIdAsync(Guid.Empty, CancellationToken.None);
    }
}
