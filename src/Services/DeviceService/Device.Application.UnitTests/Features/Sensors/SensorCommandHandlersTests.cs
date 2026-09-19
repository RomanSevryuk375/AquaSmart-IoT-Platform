using AutoMapper;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.Sensors.Command.AddSensor;
using Device.Application.Features.Sensors.Command.DeleteSensor;
using Device.Application.Features.Sensors.Command.SetSensorState;

namespace Device.Application.UnitTests.Features.Sensors;

public class SensorCommandHandlersTests
{
    private readonly ISensorRepository _sensorRepoMock = Substitute.For<ISensorRepository>();
    private readonly IMapper _mapperMock = Substitute.For<IMapper>();

    [Fact]
    public async Task AddSensorHandler_WithValidCommand_AddsToRepoAndReturnsSuccess()
    {
        // Arrange
        var handler = new AddSensorHandler(_sensorRepoMock, _mapperMock);
        var command = new AddSensorCommand
        {
            ControllerId = TestConstants.ControllerId,
            UserId = TestConstants.UserId,
            Name = "Water Temp",
            ConnectionProtocol = ConnectionProtocol.I2C,
            ConnectionAddress = TestConstants.ValidI2cAddress,
            Type = SensorType.Temperature
        };

        var responseDto = new SensorCreatedResponse { Id = Guid.NewGuid() };
        _mapperMock.Map<SensorCreatedResponse>(Arg.Any<Sensor>()).Returns(responseDto);

        // Act
        Result<SensorCreatedResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(responseDto);
        await _sensorRepoMock.Received(1).AddAsync(Arg.Any<Sensor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddSensorHandler_WithInvalidCommand_ReturnsFailure()
    {
        // Arrange
        var handler = new AddSensorHandler(_sensorRepoMock, _mapperMock);
        var command = new AddSensorCommand
        {
            ControllerId = TestConstants.ControllerId,
            UserId = TestConstants.UserId,
            Name = "", // Invalid: empty name
            ConnectionProtocol = ConnectionProtocol.OneWire,
            ConnectionAddress = "28-000000000001",
            Type = SensorType.Temperature
        };

        // Act
        Result<SensorCreatedResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _sensorRepoMock.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task DeleteSensorHandler_WhenValid_MarksAsDeletedAndDeletesFromRepo()
    {
        // Arrange
        var handler = new DeleteSensorHandler(_sensorRepoMock);
        Sensor sensor = new SensorBuilder().Build();
        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);

        var command = new DeleteSensorCommand
        {
            SensorId = sensor.Id,
            UserId = sensor.UserId
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _sensorRepoMock.Received(1).DeleteAsync(sensor.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetSensorStateHandler_WhenValid_UpdatesStateAndReturnsSuccess()
    {
        // Arrange
        var handler = new SetSensorStateHandler(_sensorRepoMock);
        Sensor sensor = new SensorBuilder().Build();
        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);

        var command = new SetSensorStateCommand
        {
            SensorId = sensor.Id,
            SensorState = SensorState.Faulty
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.State.Should().Be(SensorState.Faulty);
    }
}
