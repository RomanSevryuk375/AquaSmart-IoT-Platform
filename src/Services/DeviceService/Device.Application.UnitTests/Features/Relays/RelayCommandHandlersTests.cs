using AutoMapper;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Application.Features.RelayCommands.Command.ToggleRelayState;
using Device.Application.Features.Relays.Command.AddRelay;
using Device.Application.Features.Relays.Command.DeleteRelay;
using Device.Application.Features.Relays.Command.SetRelayPowerSensor;
using Device.Application.Features.Relays.Command.UpdateRelay;
using Microsoft.Extensions.Options;

namespace Device.Application.UnitTests.Features.Relays;

public class RelayCommandHandlersTests
{
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly ISensorRepository _sensorRepoMock = Substitute.For<ISensorRepository>();
    private readonly IRelayCommandsRepository _queueRepoMock = Substitute.For<IRelayCommandsRepository>();
    private readonly IDeviceSecurityService _securityServiceMock = Substitute.For<IDeviceSecurityService>();
    private readonly IMapper _mapperMock = Substitute.For<IMapper>();

    [Fact]
    public async Task AddRelayHandler_WithValidCommand_AddsToRepositoryAndReturnsSuccess()
    {
        // Arrange
        var handler = new AddRelayHandler(_relayRepoMock, _controllerRepoMock, _mapperMock);
        Controller controller = new ControllerBuilder().Build();
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);
        var command = new AddRelayCommand
        {
            ControllerId = controller.Id,
            UserId = TestConstants.UserId,
            PowerSensorId = null,
            Name = "Main Pump",
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = TestConstants.ValidDigitalAddress,
            IsNormallyOpen = true,
            Purpose = RelayPurpose.Pump,
            IsActive = false,
            IsManual = true
        };

        var responseDto = new RelayCreatedResponse { Id = Guid.NewGuid() };
        _mapperMock.Map<RelayCreatedResponse>(Arg.Any<Relay>()).Returns(responseDto);

        // Act
        Result<RelayCreatedResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(responseDto);
        controller.ConfigVersion.Should().Be(2);
        await _relayRepoMock.Received(1).AddAsync(Arg.Any<Relay>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddRelayHandler_WithInvalidCommand_ReturnsFailure()
    {
        // Arrange
        var handler = new AddRelayHandler(_relayRepoMock, _controllerRepoMock, _mapperMock);
        var command = new AddRelayCommand
        {
            ControllerId = TestConstants.ControllerId,
            UserId = TestConstants.UserId,
            Name = "", 
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = TestConstants.ValidDigitalAddress
        };

        // Act
        Result<RelayCreatedResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _relayRepoMock.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task UpdateRelayHandler_WhenSameController_UpdatesSuccessfully()
    {
        // Arrange
        var handler = new UpdateRelayHandler(_relayRepoMock, _controllerRepoMock, _securityServiceMock);
        Controller controller = new ControllerBuilder().Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();
        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        var command = new UpdateRelayCommand
        {
            RelayId = relay.Id,
            ControllerId = relay.ControllerId,
            UserId = relay.UserId,
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "12",
            Purpose = RelayPurpose.Light,
            IsNormallyOpen = false
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.Purpose.Should().Be(RelayPurpose.Light);
        controller.ConfigVersion.Should().Be(2);
    }

    [Fact]
    public async Task UpdateRelayHandler_WhenControllerChangedAndNotOwned_ReturnsFailure()
    {
        // Arrange
        var handler = new UpdateRelayHandler(_relayRepoMock, _controllerRepoMock, _securityServiceMock);
        Relay relay = new RelayBuilder().Build();
        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        var newControllerId = Guid.NewGuid();
        _securityServiceMock.EnsureUserOwnsControllerAsync(newControllerId, relay.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Forbidden("Access.Denied", ErrorMessages.AccessDenied)));

        var command = new UpdateRelayCommand
        {
            RelayId = relay.Id,
            ControllerId = newControllerId,
            UserId = relay.UserId,
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "12",
            Purpose = RelayPurpose.Light,
            IsNormallyOpen = false
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRelayHandler_WhenValid_MarksAsDeletedAndDeletesFromRepo()
    {
        // Arrange
        var handler = new DeleteRelayHandler(_relayRepoMock, _controllerRepoMock);
        Controller controller = new ControllerBuilder().Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();
        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _controllerRepoMock.GetByIdAsync(controller.Id, Arg.Any<CancellationToken>()).Returns(controller);

        var command = new DeleteRelayCommand
        {
            RelayId = relay.Id,
            UserId = relay.UserId
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        controller.ConfigVersion.Should().Be(2);
        await _relayRepoMock.Received(1).DeleteAsync(relay.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRelayPowerSensorHandler_WhenSensorNotFound_ReturnsNotFound()
    {
        // Arrange
        var handler = new SetRelayPowerSensorHandler(_relayRepoMock, _sensorRepoMock);
        Relay relay = new RelayBuilder().Build();
        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        var powerSensorId = Guid.NewGuid();
        _sensorRepoMock.GetByIdAsync(powerSensorId, Arg.Any<CancellationToken>()).Returns((Sensor?)null);

        var command = new SetRelayPowerSensorCommand
        {
            RelayId = relay.Id,
            PowerSensorId = powerSensorId,
            UserId = relay.UserId
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task SetRelayPowerSensorHandler_WhenSensorOnDifferentController_ReturnsValidation()
    {
        // Arrange
        var handler = new SetRelayPowerSensorHandler(_relayRepoMock, _sensorRepoMock);
        Relay relay = new RelayBuilder().Build();
        Sensor sensor = new SensorBuilder().WithControllerId(Guid.NewGuid()).WithType(SensorType.Voltage).Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);

        var command = new SetRelayPowerSensorCommand
        {
            RelayId = relay.Id,
            PowerSensorId = sensor.Id,
            UserId = relay.UserId
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task SetRelayPowerSensorHandler_WhenValid_SetsSensorAndReturnsSuccess()
    {
        // Arrange
        var handler = new SetRelayPowerSensorHandler(_relayRepoMock, _sensorRepoMock);
        Relay relay = new RelayBuilder().Build();
        Sensor sensor = new SensorBuilder().WithControllerId(relay.ControllerId).WithType(SensorType.Voltage).Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);
        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);

        var command = new SetRelayPowerSensorCommand
        {
            RelayId = relay.Id,
            PowerSensorId = sensor.Id,
            UserId = relay.UserId
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.PowerSensorId.Should().Be(sensor.Id);
    }

    [Fact]
    public async Task ToggleRelayStateHandler_WhenValid_EnqueuesPendingCommandWithoutMutatingIsActive()
    {
        // Arrange
        IOptions<DeviceSettings> options = Options.Create(new DeviceSettings { CommandTtlMinutes = 10 });
        var handler = new ToggleRelayStateHandler(_relayRepoMock, _queueRepoMock, options);
        Relay relay = new RelayBuilder().AsActive(false).Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        var command = new ToggleRelayStateCommand
        {
            RelayId = relay.Id,
            UserId = relay.UserId
        };

        // Act
        Result<RelayCommandQueuedResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CommandId.Should().NotBeEmpty();
        result.Value.RelayId.Should().Be(relay.Id);
        result.Value.TargetState.Should().BeTrue();
        result.Value.Status.Should().Be(CommandStatus.Pending.ToString());
        relay.IsActive.Should().BeFalse();
        await _queueRepoMock.Received(1).AddAsync(
            Arg.Is<RelayCommand>(c => c.TargetState && c.Status == CommandStatus.Pending),
            Arg.Any<CancellationToken>());
    }
}
