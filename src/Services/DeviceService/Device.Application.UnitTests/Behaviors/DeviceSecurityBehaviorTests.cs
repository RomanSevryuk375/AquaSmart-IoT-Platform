using Device.Application.Behaviors;

namespace Device.Application.UnitTests.Behaviors;

public class DeviceSecurityBehaviorTests
{
    public sealed record TestDeviceRequest(Guid ControllerId, string DeviceToken)
        : IRequest<Result>, IDeviceBoundRequest;

    private readonly IDeviceSecurityService _securityServiceMock;
    private readonly RequestHandlerDelegate<Result> _nextMock;
    private readonly DeviceSecurityBehavior<TestDeviceRequest, Result> _behavior;

    public DeviceSecurityBehaviorTests()
    {
        _securityServiceMock = Substitute.For<IDeviceSecurityService>();
        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
        _nextMock.Invoke().Returns(Result.Success());

        _behavior = new DeviceSecurityBehavior<TestDeviceRequest, Result>(_securityServiceMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidToken_CallsNext()
    {
        // Arrange
        var request = new TestDeviceRequest(Guid.NewGuid(), "valid_token");
        _securityServiceMock.EnsureDeviceAccessAsync(
            request.ControllerId, request.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var request = new TestDeviceRequest(Guid.NewGuid(), "invalid_token");
        _securityServiceMock.EnsureDeviceAccessAsync(
            request.ControllerId, request.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Conflict("Access.Denied", "Invalid token")));

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Access.Denied");
        await _nextMock.DidNotReceive().Invoke();
    }
}
