using BuildingBlocks.Domain.Results;
using Control.Application.Features.Relays.Commands.SyncRelayMode;

namespace Control.Application.UnitTests.Features.Relays;

public class SyncRelayModeHandlerTests
{
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly SyncRelayModeHandler _handler;

    public SyncRelayModeHandlerTests()
    {
        _handler = new SyncRelayModeHandler(_relayRepoMock);
    }

    [Fact]
    public async Task Handle_WhenRelayExists_UpdatesModeWithoutRaisingDomainEvents()
    {
        // Arrange
        Relay relay = new RelayBuilder()
            .WithIsManual(false)
            .Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        var command = new SyncRelayModeCommand
        {
            RelayId = relay.Id,
            IsManual = true
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.IsManual.Should().BeTrue();
        relay.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRelayNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        _relayRepoMock.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Relay?)null);

        var command = new SyncRelayModeCommand
        {
            RelayId = Guid.NewGuid(),
            IsManual = true
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Relay.NotFound");
    }
}
