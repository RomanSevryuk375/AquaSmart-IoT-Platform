using Contracts.Abstractions;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayMode;

internal sealed record ToggleRelayModeCommand : ICommand<bool>
{
    public Guid RelayId { get; init; }
}
