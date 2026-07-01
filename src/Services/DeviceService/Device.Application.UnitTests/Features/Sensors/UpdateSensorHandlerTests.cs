using Contracts.Enums;
using Contracts.Results;
using Device.Application.Features.Sensors.Command.UpdateSensor;
using Device.Application.Interfaces;
using Device.Domain.Entities.Sensors;
using Device.Domain.Interfaces;
using Device.TestShared.Builders;
using FluentAssertions;
using NSubstitute;

namespace Device.Application.UnitTests.Features.Sensors;

public class UpdateSensorHandlerTests
{
    private readonly ISensorRepository _sensorRepoMock = Substitute.For<ISensorRepository>();
    private readonly IDeviceSecurityService _securityServiceMock = Substitute.For<IDeviceSecurityService>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly UpdateSensorHandler _handler;

    public UpdateSensorHandlerTests()
    {
        _handler = new UpdateSensorHandler(_sensorRepoMock, _securityServiceMock, _unitOfWorkMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenMovedToNewController_ChecksSecurityAndReturnsSuccess()
    {
        // Arrange
        var oldControllerId = Guid.NewGuid();
        var newControllerId = Guid.NewGuid();

        Sensor sensor = new SensorBuilder().WithControllerId(oldControllerId).Build();

        var command = new UpdateSensorCommand
        {
            SensorId = sensor.Id,
            ControllerId = newControllerId,
            Name = "New Name",
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "D2"
        };

        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);

        _securityServiceMock.EnsureUserOwnsControllerAsync(newControllerId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.ControllerId.Should().Be(newControllerId);

        await _securityServiceMock.Received(1).EnsureUserOwnsControllerAsync(newControllerId, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenMovedToNewControllerButAccessDenied_ReturnsFailureAndDoesNotSave()
    {
        // Arrange
        var newControllerId = Guid.NewGuid();
        Sensor sensor = new SensorBuilder().Build();

        var command = new UpdateSensorCommand
        {
            SensorId = sensor.Id,
            ControllerId = newControllerId,
            Name = "New Name",
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "D2"
        };

        _sensorRepoMock.GetByIdAsync(sensor.Id, Arg.Any<CancellationToken>()).Returns(sensor);

        var expectedError = Error.Conflict("Access.Denied", "Forbidden");
        _securityServiceMock.EnsureUserOwnsControllerAsync(newControllerId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(expectedError));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(expectedError);

        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
