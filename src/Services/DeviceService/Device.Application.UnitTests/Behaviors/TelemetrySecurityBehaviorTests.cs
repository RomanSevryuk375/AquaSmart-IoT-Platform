using Contracts.Results;
using Device.Application.Behaviors;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.TestShared.Builders;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Device.Application.UnitTests.Behaviors;

public class TelemetrySecurityBehaviorTests
{
    public sealed record TestTelemetryRequest(string MacAddress, string DeviceToken)
        : IRequest<Result>, IMacAddressTokenBoundRequest;

    private readonly IControllerRepository _controllerRepoMock;
    private readonly IMyHasher _hasherMock;
    private readonly RequestHandlerDelegate<Result> _nextMock;
    private readonly TelemetrySecurityBehavior<TestTelemetryRequest, Result> _behavior;

    public TelemetrySecurityBehaviorTests()
    {
        _controllerRepoMock = Substitute.For<IControllerRepository>();
        _hasherMock = Substitute.For<IMyHasher>();
        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
        _nextMock.Invoke().Returns(Result.Success());

        _behavior = new TelemetrySecurityBehavior<TestTelemetryRequest, Result>(_controllerRepoMock, _hasherMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenControllerNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new TestTelemetryRequest("00:11:22:33:44:55", "some_token");
        _controllerRepoMock.GetByMacAddressAsync(request.MacAddress, Arg.Any<CancellationToken>())
            .Returns((Controller?)null);

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Controller.NotFound");
        await _nextMock.DidNotReceive().Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenHashMismatch_ReturnsAccessDenied()
    {
        // Arrange
        var request = new TestTelemetryRequest("00:11:22:33:44:55", "invalid_token");
        Controller controller = new ControllerBuilder().Build();

        _controllerRepoMock.GetByMacAddressAsync(request.MacAddress, Arg.Any<CancellationToken>())
            .Returns(controller);

        _hasherMock.Verify(request.DeviceToken, controller.DeviceTokenHash).Returns(false);

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Access.Denied");
        await _nextMock.DidNotReceive().Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenValid_CallsNext()
    {
        // Arrange
        var request = new TestTelemetryRequest("00:11:22:33:44:55", "valid_token");
        Controller controller = new ControllerBuilder().Build();

        _controllerRepoMock.GetByMacAddressAsync(request.MacAddress, Arg.Any<CancellationToken>())
            .Returns(controller);
        _hasherMock.Verify(request.DeviceToken, controller.DeviceTokenHash).Returns(true);

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }
}
