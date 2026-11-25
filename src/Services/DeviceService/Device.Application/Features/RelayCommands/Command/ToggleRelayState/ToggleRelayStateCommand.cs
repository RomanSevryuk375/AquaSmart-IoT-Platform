using BuildingBlocks.Domain.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayState;

public sealed record ToggleRelayStateCommand
    : ICommand<RelayCommandQueuedResponse>, IRelayBoundRequest
{
    public Guid RelayId { get; init; }
    public Guid UserId { get; init; }
}
