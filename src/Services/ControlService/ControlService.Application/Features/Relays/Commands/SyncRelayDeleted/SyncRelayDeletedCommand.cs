using Contracts.Abstractions;

namespace Control.Application.Features.Relays.Commands.SyncRelayDeleted;

public sealed record SyncRelayDeletedCommand : ICommand
{
    public Guid RelayId { get; init; }
}
