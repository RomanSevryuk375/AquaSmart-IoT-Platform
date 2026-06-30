using Contracts.Results;
using Device.Application.Behaviors;
using Device.Application.Interfaces;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Device.Application.UnitTests.Behaviors;

public class ControllerSecurityBehaviorTests
{
    public sealed record TestControllerRequest(Guid ControllerId) : IRequest<Result>, IControllerBoundRequest;

    private readonly IDeviceSecurityService _securityServiceMock;
    private readonly RequestHandlerDelegate<Result> _nextMock;
    private readonly ControllerSecurityBehavior<TestControllerRequest, Result> _behavior;

    public ControllerSecurityBehaviorTests()
    {
        _securityServiceMock = Substitute.For<IDeviceSecurityService>();
        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
        _nextMock.Invoke().Returns(Result.Success());

        _behavior = new ControllerSecurityBehavior<TestControllerRequest, Result>(_securityServiceMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserOwnsController_CallsNextAndReturnsSuccess()
    {
        // Arrange
        var request = new TestControllerRequest(Guid.NewGuid());

        _securityServiceMock.EnsureUserOwnsControllerAsync(request.ControllerId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenAccessDenied_ReturnsFailureAndShortCircuits()
    {
        // Arrange
        var request = new TestControllerRequest(Guid.NewGuid());
        var expectedError = Error.Conflict("Access.Denied", "Forbidden");

        _securityServiceMock.EnsureUserOwnsControllerAsync(request.ControllerId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(expectedError));

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(expectedError);

        await _nextMock.DidNotReceive().Invoke();
    }
}
