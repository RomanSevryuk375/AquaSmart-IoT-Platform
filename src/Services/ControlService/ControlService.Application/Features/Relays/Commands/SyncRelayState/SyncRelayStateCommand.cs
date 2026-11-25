using BuildingBlocks.Domain.Abstractions;

namespace Control.Application.Features.Relays.Commands.SyncRelayState;

public sealed record SyncRelayStateCommand : ICommand
{
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public bool IsActive { get; init; }
}
