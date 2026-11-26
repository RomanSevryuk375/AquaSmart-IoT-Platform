using AutoMapper;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Control.Application.Features.Relays.Commands.SyncRelayUpdated;
using MediatR;

namespace Control.Application.UnitTests.Features.Relays;

public class SyncRelayUpdatedHandlerTests
{
    private readonly IRelayRepository _relayRepoMock = Substitute.For<IRelayRepository>();
    private readonly ISender _senderMock = Substitute.For<ISender>();
    private readonly IMapper _mapperMock = Substitute.For<IMapper>();
    private readonly SyncRelayUpdatedHandler _handler;

    public SyncRelayUpdatedHandlerTests()
    {
        _handler = new SyncRelayUpdatedHandler(_relayRepoMock, _senderMock, _mapperMock);
    }

    [Fact]
    public async Task Handle_WhenRelayExists_SyncsStateAndModeWithoutRaisingDomainEvents()
    {
        // Arrange
        Relay relay = new RelayBuilder()
            .WithName("Old Pump")
            .WithIsActive(false)
            .WithIsManual(false)
            .Build();

        _relayRepoMock.GetByIdAsync(relay.Id, Arg.Any<CancellationToken>()).Returns(relay);

        var command = new SyncRelayUpdatedCommand
        {
            ControllerId = relay.ControllerId,
            RelayId = relay.Id,
            Name = "Main Pump",
            Purpose = RelayPurpose.Pump,
            IsActive = true,
            IsManual = true
        };

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.Name.Value.Should().Be("Main Pump");
        relay.Purpose.Should().Be(RelayPurpose.Pump);
        relay.IsActive.Should().BeTrue();
        relay.IsManual.Should().BeTrue();
        relay.DomainEvents.Should().BeEmpty();
    }
}
