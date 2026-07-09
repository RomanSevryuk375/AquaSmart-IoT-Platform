using Telemetry.Application.Features.BackgroundJobs.Commands.CheckSensorState;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.UnitTests.Features.BackgroundJobs;

public class CheckSensorStateHandlerTests
{
    private readonly ISensorRepository _sensorRepoMock;
    private readonly CheckSensorStateHandler _handler;

    public CheckSensorStateHandlerTests()
    {
        _sensorRepoMock = Substitute.For<ISensorRepository>();
        _handler = new CheckSensorStateHandler(_sensorRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenDelayedSensorsExist_MarksAsNoDataAndReturnsSuccess()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();
        _sensorRepoMock.GetDelayedSensors(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Sensor> { sensor });

        var command = new CheckSensorStateCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.State.Should().Be(SensorState.NoData);
        sensor.IsDataDelayed.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoDelayedSensors_ReturnsSuccessAndDoesNothing()
    {
        // Arrange
        _sensorRepoMock.GetDelayedSensors(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Sensor>());

        var command = new CheckSensorStateCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
