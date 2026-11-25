using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Application.Services;
using Microsoft.Extensions.Options;

namespace Device.Application.UnitTests.Services;

public class RetryRelayCommandsLogicTests
{
    private readonly IRelayCommandsRepository _commandsRepoMock = Substitute.For<IRelayCommandsRepository>();
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IMqttCommandPublisher _mqttPublisherMock = Substitute.For<IMqttCommandPublisher>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ILogger<RetryRelayCommandsService> _loggerMock = Substitute.For<ILogger<RetryRelayCommandsService>>();

    [Fact]
    public async Task RetryPendingCommands_WhenExpiredOrAttemptsExhausted_MarksCommandExpired()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { CommandRetryCooldownSeconds = 1 });
        var service = new RetryRelayCommandsService(
            _commandsRepoMock, _controllerRepoMock, _mqttPublisherMock, _unitOfWorkMock, options, _loggerMock);

        RelayCommand expiredCommand = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithExpireAt(DateTime.UtcNow.AddMinutes(-2))
            .Build();

        RelayCommand exhaustedCommand = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithExpireAt(DateTime.UtcNow.AddMinutes(10))
            .Build();
        exhaustedCommand.MarkAsSent();
        exhaustedCommand.MarkAsSent();
        exhaustedCommand.MarkAsSent();
        typeof(RelayCommand).GetProperty(nameof(RelayCommand.ProcessedAt))!
            .SetValue(exhaustedCommand, DateTime.UtcNow.AddSeconds(-10));

        _commandsRepoMock.GetActiveCommandsAsync(Arg.Any<CancellationToken>())
            .Returns([expiredCommand, exhaustedCommand]);

        // Act
        Result<int> result = await service.ProcessActiveCommandsAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        expiredCommand.Status.Should().Be(CommandStatus.Expired);
        exhaustedCommand.Status.Should().Be(CommandStatus.Expired);
        await _mqttPublisherMock.DidNotReceiveWithAnyArgs()
            .PublishRelayCommandAsync(default!, default!, default);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryPendingCommands_WhenUnackedAndControllerOnline_RepublishesToMqttAndIncrementsAttemptCount()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { CommandRetryCooldownSeconds = 10 });
        var service = new RetryRelayCommandsService(
            _commandsRepoMock, _controllerRepoMock, _mqttPublisherMock, _unitOfWorkMock, options, _loggerMock);

        Controller controller = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:10")
            .Build();

        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithExpireAt(DateTime.UtcNow.AddMinutes(10))
            .Build();
        command.MarkAsSent();
        typeof(RelayCommand).GetProperty(nameof(RelayCommand.ProcessedAt))!
            .SetValue(command, DateTime.UtcNow.AddSeconds(-30));

        _commandsRepoMock.GetActiveCommandsAsync(Arg.Any<CancellationToken>()).Returns([command]);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);
        _mqttPublisherMock.PublishRelayCommandAsync(
            controller.MacAddress.Value,
            Arg.Any<RelayCommandMqttPayload>(),
            Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result<int> result = await service.ProcessActiveCommandsAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        command.Status.Should().Be(CommandStatus.Sent);
        command.AttemptCount.Should().Be(2);
        await _mqttPublisherMock.Received(1).PublishRelayCommandAsync(
            controller.MacAddress.Value,
            Arg.Is<RelayCommandMqttPayload>(p => p.CommandId == command.Id && p.RelayId == command.RelayId),
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryPendingCommands_WhenControllerOffline_SkipsPublishWithoutExhaustingAttempts()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { CommandRetryCooldownSeconds = 10 });
        var service = new RetryRelayCommandsService(
            _commandsRepoMock, _controllerRepoMock, _mqttPublisherMock, _unitOfWorkMock, options, _loggerMock);

        Controller offlineController = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:12")
            .AsOffline()
            .Build();

        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(offlineController.Id)
            .WithExpireAt(DateTime.UtcNow.AddMinutes(10))
            .Build();

        _commandsRepoMock.GetActiveCommandsAsync(Arg.Any<CancellationToken>()).Returns([command]);
        _controllerRepoMock.GetByIdAsync(offlineController.Id, Arg.Any<CancellationToken>()).Returns(offlineController);

        // Act
        Result<int> result = await service.ProcessActiveCommandsAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        command.Status.Should().Be(CommandStatus.Pending);
        command.AttemptCount.Should().Be(0);
        await _mqttPublisherMock.DidNotReceiveWithAnyArgs()
            .PublishRelayCommandAsync(default!, default!, default);
        await _unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task RetryPendingCommands_WhenBrandNewPendingWithinCooldown_SkipsToAvoidOutboxRace()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { CommandRetryCooldownSeconds = 60 });
        var service = new RetryRelayCommandsService(
            _commandsRepoMock, _controllerRepoMock, _mqttPublisherMock, _unitOfWorkMock, options, _loggerMock);

        Controller onlineController = new ControllerBuilder()
            .WithMacAddress("AA:BB:CC:DD:EE:13")
            .Build();

        RelayCommand freshPending = new RelayCommandBuilder()
            .WithControllerId(onlineController.Id)
            .WithExpireAt(DateTime.UtcNow.AddMinutes(10))
            .Build();

        _commandsRepoMock.GetActiveCommandsAsync(Arg.Any<CancellationToken>()).Returns([freshPending]);
        _controllerRepoMock.GetByIdAsync(onlineController.Id, Arg.Any<CancellationToken>()).Returns(onlineController);

        // Act
        Result<int> result = await service.ProcessActiveCommandsAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        freshPending.AttemptCount.Should().Be(0);
        await _mqttPublisherMock.DidNotReceiveWithAnyArgs()
            .PublishRelayCommandAsync(default!, default!, default);
    }
}
