using Device.Application.Behaviors;

namespace Device.Application.UnitTests.Behaviors;

public class CommandSecurityBehaviorTests
{
    public sealed record TestCommandRequest(Guid CommandId, string DeviceToken)
        : IRequest<Result>, ICommandBoundRequest;

    private readonly IRelayCommandsRepository _commandsRepoMock;
    private readonly IDeviceSecurityService _securityServiceMock;
    private readonly RequestHandlerDelegate<Result> _nextMock;
    private readonly CommandSecurityBehavior<TestCommandRequest, Result> _behavior;

    public CommandSecurityBehaviorTests()
    {
        _commandsRepoMock = Substitute.For<IRelayCommandsRepository>();
        _securityServiceMock = Substitute.For<IDeviceSecurityService>();

        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
        _nextMock.Invoke().Returns(Result.Success());

        _behavior = new CommandSecurityBehavior<TestCommandRequest, Result>(
            _commandsRepoMock, _securityServiceMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenCommandNotFound_ReturnsNotFoundAndShortCircuits()
    {
        // Arrange
        var request = new TestCommandRequest(Guid.NewGuid(), "some_token");
        _commandsRepoMock.GetByIdAsync(request.CommandId, Arg.Any<CancellationToken>())
            .Returns((RelayCommand?)null);

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not found");

        await _securityServiceMock.DidNotReceiveWithAnyArgs().EnsureDeviceAccessAsync(Guid.Empty, "", default);
        await _nextMock.DidNotReceive().Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenAccessDenied_ReturnsFailureAndShortCircuits()
    {
        // Arrange
        var request = new TestCommandRequest(Guid.NewGuid(), "invalid_token");
        RelayCommand command = new RelayCommandBuilder().WithId(request.CommandId).Build();

        _commandsRepoMock.GetByIdAsync(request.CommandId, Arg.Any<CancellationToken>()).Returns(command);

        _securityServiceMock.EnsureDeviceAccessAsync(command.ControllerId, request.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Conflict("Access.Denied", "Invalid device token")));

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Access.Denied");
        await _nextMock.DidNotReceive().Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenCommandExistsAndAccessGranted_CallsNext()
    {
        // Arrange
        var request = new TestCommandRequest(Guid.NewGuid(), "valid_token");
        RelayCommand command = new RelayCommandBuilder().WithId(request.CommandId).Build();

        _commandsRepoMock.GetByIdAsync(request.CommandId, Arg.Any<CancellationToken>()).Returns(command);
        _securityServiceMock.EnsureDeviceAccessAsync(command.ControllerId, request.DeviceToken, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }
}
