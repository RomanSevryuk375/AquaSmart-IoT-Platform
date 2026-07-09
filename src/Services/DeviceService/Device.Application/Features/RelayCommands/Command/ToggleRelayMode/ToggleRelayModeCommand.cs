using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayMode;

public sealed record ToggleRelayModeCommand
    : ICommand<bool>, IRelayBoundRequest
{
    public Guid RelayId { get; init; }
}
