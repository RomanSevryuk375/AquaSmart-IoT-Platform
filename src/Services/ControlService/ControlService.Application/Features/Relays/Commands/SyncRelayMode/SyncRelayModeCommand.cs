using Contracts.Abstractions;

namespace Control.Application.Features.Relays.Commands.SyncRelayMode;

public sealed record SyncRelayModeCommand : ICommand
{
    public Guid RelayId { get; init; }
    public bool IsManual { get; init; }
}
