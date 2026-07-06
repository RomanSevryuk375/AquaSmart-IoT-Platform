using Contracts.Abstractions;

namespace Control.Application.Features.Relays.Commands.SyncRelayState;

public sealed record SyncRelayStateCommand : ICommand
{
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public bool TargetState { get; init; }
    public DateTime? ExpireAt { get; init; }
}
