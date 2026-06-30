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

public class RelaySecurityBehaviorTests
{
    public sealed record TestRelayRequest(Guid RelayId) : IRequest<Result>, IRelayBoundRequest;

    private readonly IRelayRepository _relayRepoMock;
    private readonly IDeviceSecurityService _securityServiceMock;
    private readonly RequestHandlerDelegate<Result> _nextMock;
    private readonly RelaySecurityBehavior<TestRelayRequest, Result> _behavior;

    public RelaySecurityBehaviorTests()
    {
        _relayRepoMock = Substitute.For<IRelayRepository>();
        _securityServiceMock = Substitute.For<IDeviceSecurityService>();

        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
        _nextMock.Invoke().Returns(Result.Success());

        _behavior = new RelaySecurityBehavior<TestRelayRequest, Result>(
            _relayRepoMock, _securityServiceMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenRelayNotFound_ReturnsNotFoundAndShortCircuits()
    {
        // Arrange
        var request = new TestRelayRequest(Guid.NewGuid());
        _relayRepoMock.GetByIdAsync(request.RelayId, Arg.Any<CancellationToken>())
            .Returns((Relay?)null);

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not found");

        await _securityServiceMock.DidNotReceiveWithAnyArgs().EnsureUserOwnsControllerAsync(Guid.Empty, default);
        await _nextMock.DidNotReceive().Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenAccessDenied_ReturnsFailureAndShortCircuits()
    {
        // Arrange
        var request = new TestRelayRequest(Guid.NewGuid());
        Relay relay = new RelayBuilder().WithId(request.RelayId).Build();

        _relayRepoMock.GetByIdAsync(request.RelayId, Arg.Any<CancellationToken>()).Returns(relay);

        _securityServiceMock.EnsureUserOwnsControllerAsync(relay.ControllerId, Arg.Any<CancellationToken>())
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
    public async Task Handle_WhenRelayExistsAndUserIsOwner_CallsNext()
    {
        // Arrange
        var request = new TestRelayRequest(Guid.NewGuid());
        Relay relay = new RelayBuilder().WithId(request.RelayId).Build();

        _relayRepoMock.GetByIdAsync(request.RelayId, Arg.Any<CancellationToken>()).Returns(relay);
        _securityServiceMock.EnsureUserOwnsControllerAsync(relay.ControllerId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Result result = await _behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }
}
