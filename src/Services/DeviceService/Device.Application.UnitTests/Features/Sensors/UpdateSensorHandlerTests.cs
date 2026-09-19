using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.Sensors.Command.UpdateSensor;

namespace Device.Application.UnitTests.Features.Sensors;

public class UpdateSensorHandlerTests
{
    private readonly ISensorRepository _sensorRepoMock = Substitute.For<ISensorRepository>();
    private readonly IDeviceSecurityService _securityServiceMock = Substitute.For<IDeviceSecurityService>();
    private readonly UpdateSensorHandler _handler;

    public UpdateSensorHandlerTests()
    {
        _handler = new UpdateSensorHandler(_sensorRepoMock, _securityServiceMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenMovedToNewController_ChecksSecurityAndReturnsSuccess()
    {
        // Arrange
        var oldControllerId = Guid.NewGuid();
        var newControllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        Sensor sensor = new SensorBuilder().WithControllerId(oldControllerId).Build();

        var command = new UpdateSensorCommand
        {
            UserId = userId,
            SensorId = sensor.Id,
            ControllerId = newControllerId,
            Name = "New Name",
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "D2"
        };

        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);

        _securityServiceMock.EnsureUserOwnsControllerAsync(newControllerId, command.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.ControllerId.Should().Be(newControllerId);

        await _securityServiceMock.Received(1).EnsureUserOwnsControllerAsync(newControllerId, command.UserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenMovedToNewControllerButAccessDenied_ReturnsFailureAndDoesNotSave()
    {
        // Arrange
        var newControllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        Sensor sensor = new SensorBuilder().Build();

        var command = new UpdateSensorCommand
        {
            UserId = userId,
            SensorId = sensor.Id,
            ControllerId = newControllerId,
            Name = "New Name",
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "D2"
        };

        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);

        var expectedError = Error.Forbidden("Access.Denied", "Forbidden");
        _securityServiceMock.EnsureUserOwnsControllerAsync(newControllerId, command.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(expectedError));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(expectedError);
    }
}
