using Contracts.Results;
using Device.Application.Behaviors;
using Device.Application.Interfaces;
using Device.Domain.Entities.Sensors;
using Device.Domain.Interfaces;
using Device.TestShared.Builders;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Device.Application.UnitTests.Behaviors;

public class SensorSecurityBehaviorTests
{
    public sealed record TestSensorRequest(Guid SensorId) : IRequest<Result>, ISensorBoundRequest;

    private readonly ISensorRepository _sensorRepoMock;
    private readonly IDeviceSecurityService _securityServiceMock;
    private readonly RequestHandlerDelegate<Result> _nextMock;
    private readonly SensorSecurityBehavior<TestSensorRequest, Result> _behavior;

    public SensorSecurityBehaviorTests()
    {
        _sensorRepoMock = Substitute.For<ISensorRepository>();
        _securityServiceMock = Substitute.For<IDeviceSecurityService>();
        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
        _nextMock.Invoke().Returns(Result.Success());

        _behavior = new SensorSecurityBehavior<TestSensorRequest, Result>(_sensorRepoMock, _securityServiceMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSensorNotFound_ReturnsNotFoundAndShortCircuits()
    {
        // Arrange
        var request = new TestSensorRequest(Guid.NewGuid());
        _sensorRepoMock.GetByIdAsync(request.SensorId, Arg.Any<CancellationToken>()).Returns((Sensor?)null);

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sensor.NotFound");
        await _securityServiceMock.DidNotReceiveWithAnyArgs().EnsureUserOwnsControllerAsync(Guid.Empty);
        await _nextMock.DidNotReceive().Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenAccessDenied_ReturnsFailure()
    {
        // Arrange
        var request = new TestSensorRequest(Guid.NewGuid());
        Sensor sensor = new SensorBuilder().WithId(request.SensorId).Build();

        _sensorRepoMock.GetByIdAsync(request.SensorId, Arg.Any<CancellationToken>()).Returns(sensor);

        _securityServiceMock.EnsureUserOwnsControllerAsync(sensor.ControllerId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Conflict("Access.Denied", "Forbidden")));

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Access.Denied");
        await _nextMock.DidNotReceive().Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenSensorExistsAndAccessGranted_CallsNext()
    {
        // Arrange
        var request = new TestSensorRequest(Guid.NewGuid());
        Sensor sensor = new SensorBuilder().WithId(request.SensorId).Build();

        _sensorRepoMock.GetByIdAsync(request.SensorId, Arg.Any<CancellationToken>()).Returns(sensor);
        _securityServiceMock.EnsureUserOwnsControllerAsync(sensor.ControllerId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }
}
