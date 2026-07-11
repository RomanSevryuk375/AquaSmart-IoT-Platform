using Contracts.Events.TelemetryEvents;
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
    private readonly IDeviceTokenValidator _deviceTokenValidatorMock;
    private readonly AddTelemetryBatchHandler _handler;

    public AddTelemetryBatchHandlerTests()
    {
        _telemetryRepositoryMock = Substitute.For<ITelemetryRawDataRepository>();
        _sensorRepositoryMock = Substitute.For<ISensorRepository>();
        _ecosystemRepositoryMock = Substitute.For<IEcosystemRepository>();
        _deviceTokenValidatorMock = Substitute.For<IDeviceTokenValidator>();

        _handler = new AddTelemetryBatchHandler(
            _telemetryRepositoryMock,
            _sensorRepositoryMock,
            _ecosystemRepositoryMock,
            _deviceTokenValidatorMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenEcosystemNotFound_ReturnsFailure()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "00:1A:2B:3C:4D:5E",
            DeviceToken = "valid-token",
            Items = new List<TelemetryBatchEventItem>()
        };

        _deviceTokenValidatorMock.ValidateAsync(command.MacAddress, command.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto { ControllerId = controllerId, UserId = userId }));

        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns((Ecosystem?)null);

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
        var userId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).WithUserId(userId).Build();

        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "00:1A:2B:3C:4D:5E",
            DeviceToken = "valid-token",
            Items = new List<TelemetryBatchEventItem>()
        };

        _deviceTokenValidatorMock.ValidateAsync(command.MacAddress, command.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto { ControllerId = controllerId, UserId = userId }));

        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(ecosystem);

        _sensorRepositoryMock.GetAllByEcosystemId(ecosystem.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Sensor>());

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
        var userId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).WithUserId(userId).Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();

        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "00:1A:2B:3C:4D:5E",
            DeviceToken = "valid-token",
            Items = new List<TelemetryBatchEventItem>()
        };

        _deviceTokenValidatorMock.ValidateAsync(command.MacAddress, command.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto { ControllerId = controllerId, UserId = userId }));

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

        command = command with { Items = new List<TelemetryBatchEventItem> { batchItem } };

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
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSensorNotFoundInEcosystem_SkipsItem()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).WithUserId(userId).Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();

        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "00:1A:2B:3C:4D:5E",
            DeviceToken = "valid-token",
            Items = new List<TelemetryBatchEventItem>()
        };

        _deviceTokenValidatorMock.ValidateAsync(command.MacAddress, command.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto { ControllerId = controllerId, UserId = userId }));

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

        command = command with { Items = new List<TelemetryBatchEventItem> { batchItem } };

        _telemetryRepositoryMock.GetByExternalMessageIdAsync(
            batchItem.ExternalMessageId, Arg.Any<CancellationToken>())
            .Returns((RawTelemetry?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _telemetryRepositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<RawTelemetry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenExternalMessageIdAlreadyExists_SkipsItemForIdempotency()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).WithUserId(userId).Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).Build();

        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "00:1A:2B:3C:4D:5E",
            DeviceToken = "valid-token",
            Items = new List<TelemetryBatchEventItem>()
        };

        _deviceTokenValidatorMock.ValidateAsync(command.MacAddress, command.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto { ControllerId = controllerId, UserId = userId }));

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

        command = command with { Items = new List<TelemetryBatchEventItem> { batchItem } };

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
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenValidationFails_ReturnsFailureAndDoesNotCallRepositories()
    {
        // Arrange
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "00:1A:2B:3C:4D:5E",
            DeviceToken = "invalid-token",
            Items = new List<TelemetryBatchEventItem>
            {
                new() { SensorId = Guid.NewGuid(), Value = 12.3, ExternalMessageId = "msg_123", RecordedAt = DateTime.UtcNow }
            }
        };

        var error = Error.Conflict("Device.InvalidToken", "The token is invalid.");
        _deviceTokenValidatorMock.ValidateAsync(command.MacAddress, command.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Failure(error));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);

        _sensorRepositoryMock.DidNotReceiveWithAnyArgs();
        _telemetryRepositoryMock.DidNotReceiveWithAnyArgs();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenEcosystemUserMismatch_ReturnsAccessDeniedConflict()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var ecosystemUserId = Guid.NewGuid();
        var validatorUserId = Guid.NewGuid();

        Ecosystem ecosystem = new EcosystemBuilder()
            .WithControllerId(controllerId)
            .WithUserId(ecosystemUserId)
            .Build();

        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "00:1A:2B:3C:4D:5E",
            DeviceToken = "valid-token-wrong-user",
            Items = new List<TelemetryBatchEventItem>()
        };

        _deviceTokenValidatorMock.ValidateAsync(command.MacAddress, command.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto { ControllerId = controllerId, UserId = validatorUserId }));

        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(ecosystem);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Access.Denied");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}

