using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayMode;

internal sealed record ToggleRelayModeCommand
    : ICommand<bool>, IRelayBoundRequest
{
    public Guid RelayId { get; init; }
}
