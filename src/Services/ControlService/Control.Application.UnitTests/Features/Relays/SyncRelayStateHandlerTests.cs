using Control.Application.Features.Relays.Commands.SyncRelayState;

namespace Control.Application.UnitTests.Features.Relays;

public class SyncRelayStateHandlerTests
{
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly SyncRelayStateHandler _handler;

    public SyncRelayStateHandlerTests()
    {
        _handler = new SyncRelayStateHandler(_relayRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenRelayExists_UpdatesStateAndReturnsSuccess()
    {
        // Arrange
        Relay relay = new RelayBuilder()
            .WithIsActive(false)
            .Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        var command = new SyncRelayStateCommand
        {
            ControllerId = relay.ControllerId,
            RelayId = relay.Id,
            TargetState = true
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.IsActive.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenRelayNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        _relayRepoMock.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Relay?)null);

        var command = new SyncRelayStateCommand
        {
            ControllerId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            TargetState = true
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Relay.NotFound");
    }
}
