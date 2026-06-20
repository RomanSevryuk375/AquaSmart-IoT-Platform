using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayState;

internal sealed record ToggleRelayStateCommand
    : ICommand<bool>, IRelayBoundRequest
{
    public Guid RelayId { get; init; }
}
